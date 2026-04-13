using ASP_PM.Models;
using ASP_PM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ASP_PM.Controllers;

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

    public async Task<IActionResult> SearchEmployees(string term)
    {
        var employees = await _employeeService.GetAllAsync();
        if (!string.IsNullOrEmpty(term))
        {
            employees = employees.Where(e =>
                e.FullName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                e.Email.Contains(term, StringComparison.OrdinalIgnoreCase));
        }
        var result = employees.Select(e => new { id = e.Id, text = e.FullName });
        return Json(result);
    }

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