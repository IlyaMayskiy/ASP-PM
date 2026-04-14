using ASP_PM.Models;
using ASP_PM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ASP_PM.Controllers;

/// <summary>
/// Five‑step wizard for creating a new project. Keeps data in TempData between steps.
/// Step 5 handles drag‑&‑drop file uploads and moves them to final storage on finish.
/// </summary>
[Authorize(Roles = "Director,ProjectManager")]
public class ProjectWizardController : Controller
{
    private readonly IEmployeeService _employeeService;
    private readonly IProjectService _projectService;
    private readonly IWebHostEnvironment _env;
    private readonly UserManager<AppUser> _userManager;

    public ProjectWizardController(IEmployeeService employeeService, IProjectService projectService, IWebHostEnvironment env, UserManager<AppUser> userManager)
    {
        _employeeService = employeeService;
        _projectService = projectService;
        _env = env;
        _userManager = userManager;
    }

    /// <summary>Entry point – shows the first step of the wizard.</summary>
    public async Task<IActionResult> Wizard()
    {
        var model = new ProjectWizardModel();
        var user = await _userManager.GetUserAsync(User);
        if (User.IsInRole("ProjectManager"))
        {
            var employee = await _employeeService.GetByAppUserIdAsync(user.Id);
            if (employee != null)
            {
                model.ProjectManagerId = employee.Id;
            }
            ViewBag.IsManager = true;
        }
        else
        {
            ViewBag.IsManager = false;
        }
        return View(model);
    }

    /// <summary>The AJAX endpoint for Select2 searches for employees by name/email and filters by role.</summary>
    public async Task<IActionResult> SearchEmployees(string term, string role = null)
    {
        var employees = await _employeeService.GetAllAsync();
        if (!string.IsNullOrEmpty(term))
        {
            employees = employees.Where(e =>
                e.FullName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                e.Email.Contains(term, StringComparison.OrdinalIgnoreCase));
        }
        
        var filtered = new List<Employee>();
        foreach (var emp in employees)
        {       
            var user = await _userManager.FindByIdAsync(emp.AppUserId);
            var userRoles = await _userManager.GetRolesAsync(user);
            
            if (string.IsNullOrEmpty(role))
                filtered.Add(emp);
            else if (role == "manager" && (userRoles.Contains("Director") || userRoles.Contains("ProjectManager")))
                filtered.Add(emp);
            else if (role == "executor" && userRoles.Contains("Employee"))
                filtered.Add(emp);
        }
        
        var result = filtered.Select(e => new { id = e.Id, text = e.FullName });
        return Json(result);
    }

    /// <summary>Accepts uploaded files via drag & drop, stores them temporarily, and returns their generated names.</summary>
    [HttpPost]
    public async Task<IActionResult> UploadFiles(List<IFormFile> files)
    {
        var tempFolder = Path.Combine(_env.WebRootPath, "temp");
        if (!Directory.Exists(tempFolder))
            Directory.CreateDirectory(tempFolder);

        var fileNames = new List<string>();
        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(tempFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                    await file.CopyToAsync(stream);
                fileNames.Add(fileName);
            }
        }
        TempData["UploadedFiles"] = JsonSerializer.Serialize(fileNames);
        return Json(new { success = true, files = fileNames });
    }

    /// <summary>Final step: creates the project, moves uploaded files to permanent location, and saves document records.</summary>
    [HttpPost]
    public async Task<IActionResult> Finish(ProjectWizardModel model)
    {
        
        if (!ModelState.IsValid)
            return View("Wizard", model);

        if (User.IsInRole("ProjectManager") && model.ProjectManagerId == null)
        {
            var user = await _userManager.GetUserAsync(User);
            var employee = await _employeeService.GetByAppUserIdAsync(user.Id);
            if (employee != null)
                model.ProjectManagerId = employee.Id;
        }

        if (model.SelectedExecutors == null || !model.SelectedExecutors.Any())
        {
            ModelState.AddModelError("", "Please select at least one executor.");
            return View("Wizard", model);
        }

        var project = new Project
        {
            Name = model.Name,
            ClientCompany = model.ClientCompany,
            ExecutorCompany = model.ExecutorCompany,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            Priority = model.Priority,
            ProjectManagerId = model.ProjectManagerId
        };

        var createdProject = await _projectService.CreateAsync(project, model.SelectedExecutors);

        
        var uploadedFilesJson = TempData["UploadedFiles"] as string;
        if (!string.IsNullOrEmpty(uploadedFilesJson))
        {
            var fileNames = JsonSerializer.Deserialize<List<string>>(uploadedFilesJson);
            if (fileNames != null)
            {
                var tempFolder = Path.Combine(_env.WebRootPath, "temp");
                var projectFolder = Path.Combine(_env.WebRootPath, "project_docs", createdProject.Id.ToString());
                if (!Directory.Exists(projectFolder))
                    Directory.CreateDirectory(projectFolder);

                foreach (var fileName in fileNames)
                {
                    var tempPath = Path.Combine(tempFolder, fileName);
                    if (System.IO.File.Exists(tempPath))
                    {
                        var destPath = Path.Combine(projectFolder, fileName);
                        System.IO.File.Move(tempPath, destPath);
                        await _projectService.AddDocumentAsync(createdProject.Id, fileName, fileName);
                    }
                }
            }
            TempData.Remove("UploadedFiles");
        }

        return RedirectToAction("Index", "Projects");
    }
}