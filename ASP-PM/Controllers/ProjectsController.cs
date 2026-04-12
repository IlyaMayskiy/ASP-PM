using ASP_PM.Models;
using ASP_PM.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ASP_PM.Controllers;

public class ProjectsController : Controller
{
    private readonly IProjectService _projectService;
    private readonly IEmployeeService _employeeService;
    private readonly ITaskService _taskService;

    public ProjectsController(IProjectService projectService, IEmployeeService employeeService, ITaskService taskService)
    {
        _projectService = projectService;
        _employeeService = employeeService;
        _taskService = taskService;
    }

    // GET: Projects
    public async Task<IActionResult> Index(DateTime? startDateFrom, DateTime? startDateTo, int? priority, string sortBy = "startdate", bool ascending = true)
    {
        var projects = await _projectService.GetFilteredAsync(startDateFrom, startDateTo, priority, sortBy, ascending);
        return View(projects);
    }

    // GET: Projects/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var project = await _projectService.GetByIdAsync(id);
        if (project == null) return NotFound();
        var tasks = await _projectService.GetTasksByProjectIdAsync(id);
        ViewBag.Tasks = tasks;
        return View(project);
    }

    // GET: Projects/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var project = await _projectService.GetByIdAsync(id);
        if (project == null) return NotFound();
        var selectedExecutors = project.Executors.Select(e => e.Id).ToArray();
        await PopulateSelectLists(project.ProjectManagerId, selectedExecutors);
        ViewBag.AllEmployees = await _employeeService.GetAllAsync();
        ViewBag.SelectedExecutorIds = selectedExecutors;
        return View(project);
    }

    // POST: Projects/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Project project, int[] selectedExecutors)
    {
        if (id != project.Id) return BadRequest();
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

    // POST: Projects/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _projectService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> GetFiles(int projectId)
    {
        var files = await _projectService.GetDocumentsAsync(projectId);
        return Json(files.Select(f => new { f.Id, f.OriginalName, f.UploadDate }));
    }

    [HttpPost]
    public async Task<IActionResult> UploadFile(int projectId, IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest();
        var doc = await _projectService.AddDocumentAsync(projectId, file);
        return Json(new { success = true, doc });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteFile(int id)
    {
        var result = await _projectService.DeleteDocumentAsync(id);
        return Json(new { success = result });
    }

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
    public async Task<IActionResult> UpdateTaskStatus(int taskId, TaskState status)
    {
        var task = await _taskService.GetByIdAsync(taskId);
        if (task == null) return NotFound();
        task.Status = status;
        await _taskService.UpdateAsync(taskId, task);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> DeleteTask(int taskId)
    {
        await _taskService.DeleteAsync(taskId);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> AddTaskToProject(int projectId, string name, int authorId, int? executorId, int priority, string? comment)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Task name is required");

        var author = await _employeeService.GetByIdAsync(authorId);
        if (author == null)
            return BadRequest("Author not found");

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