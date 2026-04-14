using ASP_PM.Models;
using ASP_PM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ASP_PM.Controllers;

/// <summary>
/// Standalone CRUD for tasks (bonus task #1). Access: Director and ProjectManager can do everything, Employee only views.
/// </summary>
[Authorize]
public class TasksController : Controller
{
    private readonly ITaskService _taskService;
    private readonly IProjectService _projectService;
    private readonly IEmployeeService _employeeService;
    private readonly UserManager<AppUser> _userManager;

    public TasksController(ITaskService taskService, IProjectService projectService, IEmployeeService employeeService, UserManager<AppUser> userManager)
    {
        _taskService = taskService;
        _projectService = projectService;
        _employeeService = employeeService;
        _userManager = userManager;
    }

    /// <summary>List of tasks with filtering by project and status, and sorting by name, priority or status.</summary>
    public async Task<IActionResult> Index(int? projectId, TaskState? status, string sortBy = "name", bool ascending = true)
    {
        var user = await _userManager.GetUserAsync(User);
        var isDirector = User.IsInRole("Director");
        var isManager = User.IsInRole("ProjectManager");
        var isEmployee = User.IsInRole("Employee");

        IEnumerable<TaskItem> tasks;
        if (isDirector)
        {
            tasks = await _taskService.GetFilteredAsync(projectId, status, sortBy, ascending);
        }
        else if (isManager)
        {
            var employee = await _employeeService.GetByAppUserIdAsync(user.Id);
            if (employee != null)
            {
                var managerProjects = await _projectService.GetProjectsByManagerIdAsync(employee.Id, null, null, null, "Id", true);
                var projectIds = managerProjects.Select(p => p.Id).ToList();
                var allTasks = await _taskService.GetFilteredAsync(projectId, status, sortBy, ascending);
                tasks = allTasks.Where(t => projectIds.Contains(t.ProjectId));
            }
            else tasks = new List<TaskItem>();
        }
        else
        {
            var employee = await _employeeService.GetByAppUserIdAsync(user.Id);
            if (employee != null)
            {
                var allTasks = await _taskService.GetFilteredAsync(projectId, status, sortBy, ascending);
                tasks = allTasks.Where(t => t.ExecutorId == employee.Id || t.AuthorId == employee.Id);
            }
            else tasks = new List<TaskItem>();
        }

        ViewBag.Projects = new SelectList(await _projectService.GetAllAsync(), "Id", "Name");
        ViewBag.Statuses = Enum.GetValues(typeof(TaskState)).Cast<TaskState>().Select(s => new SelectListItem { Value = s.ToString(), Text = s.ToString() });
        return View(tasks);
    }

    /// <summary>Form to create a new task. Optional projectId pre-selects the project.</summary>
    public async Task<IActionResult> Create(int? projectId)
    {
        ViewBag.Projects = new SelectList(await _projectService.GetAllAsync(), "Id", "Name", projectId);
        ViewBag.Employees = new SelectList(await _employeeService.GetAllAsync(), "Id", "FullName");
        var task = new TaskItem();
        if (projectId.HasValue) task.ProjectId = projectId.Value;
        return View(task);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Director,ProjectManager")]
    public async Task<IActionResult> Create(TaskItem task)
    {
        if (ModelState.IsValid)
        {
            await _taskService.CreateAsync(task);
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Projects = new SelectList(await _projectService.GetAllAsync(), "Id", "Name", task.ProjectId);
        ViewBag.Employees = new SelectList(await _employeeService.GetAllAsync(), "Id", "FullName", task.AuthorId);
        return View(task);
    }

    /// <summary>Edit form. Managers and directors can edit all fields; employees should not reach this (but if they do, they'll get a 403).</summary>
    public async Task<IActionResult> Edit(int id)
    {
        var task = await _taskService.GetByIdAsync(id);
        if (task == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        var isDirector = User.IsInRole("Director");
        var isManager = User.IsInRole("ProjectManager");
        if (!isDirector && !isManager)
        {
            return Forbid();
        }

        ViewBag.Projects = new SelectList(await _projectService.GetAllAsync(), "Id", "Name", task.ProjectId);
        ViewBag.Employees = new SelectList(await _employeeService.GetAllAsync(), "Id", "FullName", task.AuthorId);
        return View(task);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Director,ProjectManager")]
    public async Task<IActionResult> Edit(int id, TaskItem task)
    {
        if (id != task.Id) return BadRequest();
        if (ModelState.IsValid)
        {
            var updated = await _taskService.UpdateAsync(id, task);
            if (updated == null) return NotFound();
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Projects = new SelectList(await _projectService.GetAllAsync(), "Id", "Name", task.ProjectId);
        ViewBag.Employees = new SelectList(await _employeeService.GetAllAsync(), "Id", "FullName", task.AuthorId);
        return View(task);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Director,ProjectManager")]
    public async Task<IActionResult> Delete(int id)
    {
        await _taskService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}