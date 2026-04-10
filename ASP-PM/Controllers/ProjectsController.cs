using ASP_PM.Models;
using ASP_PM.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ASP_PM.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly IProjectService _projectService;
        private readonly IEmployeeService _employeeService;

        public ProjectsController(IProjectService projectService, IEmployeeService employeeService)
        {
            _projectService = projectService;
            _employeeService = employeeService;
        }

        // GET: Projects
        public async Task<IActionResult> Index(DateTime? startDateFrom, DateTime? startDateTo, int? priority, string sortBy = "startdate", bool ascending = true)
        {
            var projects = await _projectService.GetFilteredAsync(startDateFrom, startDateTo, priority, sortBy, ascending);
            return View(projects);
        }

        private async Task PopulateSelectLists(int? selectedManagerId = null, int[]? selectedExecutors = null)
        {
            var employees = await _employeeService.GetAllAsync();
            ViewData["ProjectManagerId"] = new SelectList(employees, "Id", "FullName", selectedManagerId);
            ViewBag.Executors = new MultiSelectList(employees, "Id", "FullName", selectedExecutors);
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
    }
}