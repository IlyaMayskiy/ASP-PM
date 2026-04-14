using ASP_PM.Models;
using ASP_PM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ASP_PM.Controllers;

/// <summary>
/// Heart of the app – project CRUD, filtering, documents, and inline task management.
/// Access rules: Director sees everything, ProjectManager sees only his projects, Employee sees only projects where he is an executor.
/// </summary>
[Authorize]
public class ProjectsController : Controller
{
    private readonly IProjectService _projectService;
    private readonly IEmployeeService _employeeService;
    private readonly ITaskService _taskService;
    private readonly UserManager<AppUser> _userManager;

    public ProjectsController(IProjectService projectService, IEmployeeService employeeService, ITaskService taskService, UserManager<AppUser> userManager)
    {
        _projectService = projectService;
        _employeeService = employeeService;
        _taskService = taskService;
        _userManager = userManager;
    }

    /// <summary>Displays projects with filtering (date range, priority) and sorting. Results depend on user's role.</summary>
    public async Task<IActionResult> Index(DateTime? startDateFrom, DateTime? startDateTo, int? priority, string sortBy = "startdate", bool ascending = true)
    {
        var user = await _userManager.GetUserAsync(User);
        var isDirector = User.IsInRole("Director");
        var isManager = User.IsInRole("ProjectManager");
        var isEmployee = User.IsInRole("Employee");

        IEnumerable<Project> projects;
        if (isDirector)
        {
            projects = await _projectService.GetFilteredAsync(startDateFrom, startDateTo, priority, sortBy, ascending);
        }
        else if (isManager)
        {
            var employee = await _employeeService.GetByAppUserIdAsync(user.Id);
            if (employee != null)
                projects = await _projectService.GetProjectsByManagerIdAsync(employee.Id, startDateFrom, startDateTo, priority, sortBy, ascending);
            else
                projects = new List<Project>();
        }
        else
        {
            var employee = await _employeeService.GetByAppUserIdAsync(user.Id);
            if (employee != null)
                projects = await _projectService.GetProjectsByExecutorIdAsync(employee.Id, startDateFrom, startDateTo, priority, sortBy, ascending);
            else
                projects = new List<Project>();
        }

        var currentEmployee = await _employeeService.GetByAppUserIdAsync(user?.Id);
        ViewBag.CurrentEmployeeId = currentEmployee?.Id;

        return View(projects);
    }

     /// <summary>Shows project details along with its tasks.</summary>
    public async Task<IActionResult> Details(int id)
    {
        var project = await _projectService.GetByIdAsync(id);
        if (project == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        var isDirector = User.IsInRole("Director");
        var isManager = User.IsInRole("ProjectManager");
        var isEmployee = User.IsInRole("Employee");
        var employee = await _employeeService.GetByAppUserIdAsync(user.Id);

        if (!isDirector && !(isManager && project.ProjectManagerId == employee?.Id) && !(isEmployee && project.Executors.Any(e => e.Id == employee?.Id)))
            return Forbid();

        var tasks = await _projectService.GetTasksByProjectIdAsync(id);
        ViewBag.Tasks = tasks;
        return View(project);
    }

    /// <summary>Edit form for a project. Managers can edit only their own projects.</summary>
    [Authorize(Roles = "Director,ProjectManager")]
    public async Task<IActionResult> Edit(int id)
    {
        var project = await _projectService.GetByIdAsync(id);
        if (project == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (User.IsInRole("ProjectManager"))
        {
            var employee = await _employeeService.GetByAppUserIdAsync(user.Id);
            if (project.ProjectManagerId != employee?.Id)
                return Forbid();
        }

        var selectedExecutors = project.Executors.Select(e => e.Id).ToArray();
        await PopulateSelectLists(project.ProjectManagerId, selectedExecutors);
        ViewBag.AllEmployees = await _employeeService.GetAllAsync();
        ViewBag.SelectedExecutorIds = selectedExecutors;
        return View(project);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Director,ProjectManager")]
    public async Task<IActionResult> Edit(int id, Project project, int[] selectedExecutors)
    {
        if (id != project.Id) return BadRequest();

        var user = await _userManager.GetUserAsync(User);
        if (User.IsInRole("ProjectManager"))
        {
            var employee = await _employeeService.GetByAppUserIdAsync(user.Id);
            var existing = await _projectService.GetByIdAsync(id);
            if (existing.ProjectManagerId != employee?.Id)
                return Forbid();
        }

        if (ModelState.IsValid)
        {
            var updated = await _projectService.UpdateAsync(id, project, selectedExecutors ?? Array.Empty<int>());
            if (updated == null) return NotFound();
            return RedirectToAction(nameof(Index));
        }
        await PopulateSelectLists(project.ProjectManagerId, selectedExecutors);
        ViewBag.AllEmployees = await _employeeService.GetAllAsync();
        return View(project);
    }

    /// <summary>Only directors can delete a project.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Director,ProjectManager")]
    public async Task<IActionResult> Delete(int id)
    {
        var project = await _projectService.GetByIdAsync(id);
        if (project == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (User.IsInRole("ProjectManager"))
        {
            var employee = await _employeeService.GetByAppUserIdAsync(user.Id);
            if (project.ProjectManagerId != employee?.Id)
                return Forbid();
        }

        await _projectService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    //Document
    [HttpGet]
    public async Task<IActionResult> GetFiles(int projectId)
    {
        var files = await _projectService.GetDocumentsAsync(projectId);
        return Json(files.Select(f => new { f.Id, f.OriginalName, f.UploadDate }));
    }

    [HttpPost]
    [Authorize(Roles = "Director,ProjectManager")]
    public async Task<IActionResult> UploadFile(int projectId, IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest();
        var doc = await _projectService.AddDocumentAsync(projectId, file);
        return Json(new { success = true, doc });
    }

    [HttpPost]
    [Authorize(Roles = "Director,ProjectManager")]
    public async Task<IActionResult> DeleteFile(int id)
    {
        var result = await _projectService.DeleteDocumentAsync(id);
        return Json(new { success = result });
    }
    //Tasks crud in project
    [HttpGet]
    public async Task<IActionResult> GetProjectTasks(int projectId)
    {
        var tasks = await _projectService.GetTasksByProjectIdAsync(projectId);
        return Json(tasks.Select(t => new
        {
            t.Id,
            t.Name,
            Status = t.Status.ToString(),
            t.Priority,
            ExecutorName = t.Executor?.FullName ?? "",
            t.Comment
        }));
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> UpdateTaskStatus(int taskId, TaskState status)
    {
        var task = await _taskService.GetByIdAsync(taskId);
        if (task == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        if (User.IsInRole("Director"))
        {
            task.Status = status;
            await _taskService.UpdateAsync(taskId, task);
            return Ok();
        }

        if (User.IsInRole("ProjectManager"))
        {
            var project = await _projectService.GetByIdAsync(task.ProjectId);
            var employee = await _employeeService.GetByAppUserIdAsync(user.Id);
            if (project != null && employee != null && project.ProjectManagerId == employee.Id)
            {
                task.Status = status;
                await _taskService.UpdateAsync(taskId, task);
                return Ok();
            }
            return Forbid();
        }

        if (User.IsInRole("Employee"))
        {
            var employee = await _employeeService.GetByAppUserIdAsync(user.Id);
            if (employee != null && task.ExecutorId == employee.Id)
            {
                task.Status = status;
                await _taskService.UpdateAsync(taskId, task);
                return Ok();
            }
            return Forbid();
        }

        return Forbid();
    }

    [HttpPost]
    [Authorize(Roles = "Director,ProjectManager")]
    public async Task<IActionResult> DeleteTask(int taskId)
    {
        var task = await _taskService.GetByIdAsync(taskId);
        if (task == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (User.IsInRole("ProjectManager"))
        {
            var employee = await _employeeService.GetByAppUserIdAsync(user.Id);
            var project = await _projectService.GetByIdAsync(task.ProjectId);
            if (project == null || project.ProjectManagerId != employee?.Id)
                return Forbid();
        }

        await _taskService.DeleteAsync(taskId);
        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "Director,ProjectManager")]
    public async Task<IActionResult> AddTaskToProject(int projectId, string name, int authorId, int? executorId, int priority, string? comment)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Task name is required");

        var author = await _employeeService.GetByIdAsync(authorId);
        if (author == null)
            return BadRequest("Author not found");

        var user = await _userManager.GetUserAsync(User);
        if (User.IsInRole("ProjectManager"))
        {
            var employee = await _employeeService.GetByAppUserIdAsync(user.Id);
            var project = await _projectService.GetByIdAsync(projectId);
            if (project == null || project.ProjectManagerId != employee?.Id)
                return Forbid();
        }

        var task = new TaskItem
        {
            Name = name,
            ProjectId = projectId,
            AuthorId = authorId,
            ExecutorId = executorId,
            Priority = priority,
            Comment = comment,
            Status = TaskState.ToDo
        };
        await _taskService.CreateAsync(task);
        return Ok();
    }

    private async Task PopulateSelectLists(int? selectedManagerId = null, int[]? selectedExecutors = null)
    {
        var employees = await _employeeService.GetAllAsync();
        ViewData["ProjectManagerId"] = new SelectList(employees, "Id", "FullName", selectedManagerId);
        ViewBag.Executors = new MultiSelectList(employees, "Id", "FullName", selectedExecutors);
    }
}