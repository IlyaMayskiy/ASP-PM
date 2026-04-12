using ASP_PM.Models;
using ASP_PM.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ASP_PM.Controllers;

public class TasksController : Controller
{
    private readonly ITaskService _taskService;
    private readonly IProjectService _projectService;
    private readonly IEmployeeService _employeeService;

    public TasksController(ITaskService taskService, IProjectService projectService, IEmployeeService employeeService)
    {
        _taskService = taskService;
        _projectService = projectService;
        _employeeService = employeeService;
    }

    // GET: Tasks
    public async Task<IActionResult> Index(int? projectId, TaskState? status, string sortBy = "name", bool ascending = true)
    {
        var tasks = await _taskService.GetFilteredAsync(projectId, status, sortBy, ascending);
        ViewBag.Projects = new SelectList(await _projectService.GetAllAsync(), "Id", "Name");
        ViewBag.Statuses = Enum.GetValues(typeof(TaskState)).Cast<TaskState>().Select(s => new SelectListItem { Value = s.ToString(), Text = s.ToString() });
        return View(tasks);
    }

    // GET: Tasks/Create
    public async Task<IActionResult> Create(int? projectId)
    {
        ViewBag.Projects = new SelectList(await _projectService.GetAllAsync(), "Id", "Name", projectId);
        ViewBag.Employees = new SelectList(await _employeeService.GetAllAsync(), "Id", "FullName");
        var task = new TaskItem();
        if (projectId.HasValue) task.ProjectId = projectId.Value;
        return View(task);
    }

    // POST: Tasks/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
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

    // GET: Tasks/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var task = await _taskService.GetByIdAsync(id);
        if (task == null) return NotFound();
        ViewBag.Projects = new SelectList(await _projectService.GetAllAsync(), "Id", "Name", task.ProjectId);
        ViewBag.Employees = new SelectList(await _employeeService.GetAllAsync(), "Id", "FullName", task.AuthorId);
        return View(task);
    }

    // POST: Tasks/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TaskItem task)
    {
        if (id != task.Id) return BadRequest();
        if (ModelState.IsValid)
        {
            await _taskService.UpdateAsync(id, task);
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Projects = new SelectList(await _projectService.GetAllAsync(), "Id", "Name", task.ProjectId);
        ViewBag.Employees = new SelectList(await _employeeService.GetAllAsync(), "Id", "FullName", task.AuthorId);
        return View(task);
    }

    // POST: Tasks/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _taskService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}