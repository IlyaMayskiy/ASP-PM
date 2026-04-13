using ASP_PM.Models;
using ASP_PM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ASP_PM.Controllers;

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

    public async Task<IActionResult> Index(int? projectId, TaskState? status, string sortBy = "name", bool ascending = true)
    {
        var tasks = await _taskService.GetFilteredAsync(projectId, status, sortBy, ascending);
        ViewBag.Projects = new SelectList(await _projectService.GetAllAsync(), "Id", "Name");
        ViewBag.Statuses = Enum.GetValues(typeof(TaskState)).Cast<TaskState>().Select(s => new SelectListItem { Value = s.ToString(), Text = s.ToString() });
        return View(tasks);
    }

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