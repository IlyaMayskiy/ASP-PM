using ASP_PM.Models;
using ASP_PM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASP_PM.Controllers;

[Authorize(Roles = "Director")]
public class EmployeesController : Controller
{
    private readonly IEmployeeService _employeeService;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public EmployeesController(IEmployeeService employeeService, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _employeeService = employeeService;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        var employees = await _employeeService.GetAllAsync();
        var employeeRoles = new List<(Employee Employee, string Role)>();
        foreach (var emp in employees)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.EmployeeId == emp.Id);
            var role = user != null ? (await _userManager.GetRolesAsync(user)).FirstOrDefault() : "No user";
            employeeRoles.Add((emp, role ?? "None"));
        }
        return View(employeeRoles);
    }

    public IActionResult Create()
    {
        ViewBag.Roles = new List<string> { "Employee", "ProjectManager" };
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string firstName, string secondName, string? patronymic, string email, string password, string confirmPassword, string role)
    {
        ViewBag.Roles = new List<string> { "Employee", "ProjectManager" };
        ViewBag.FirstName = firstName;
        ViewBag.SecondName = secondName;
        ViewBag.Patronymic = patronymic;
        ViewBag.Email = email;
        ViewBag.SelectedRole = role;

        if (string.IsNullOrWhiteSpace(firstName))
            ModelState.AddModelError("FirstName", "First name is required");
        if (string.IsNullOrWhiteSpace(email))
            ModelState.AddModelError("Email", "Email is required");
        if (string.IsNullOrWhiteSpace(password))
            ModelState.AddModelError("Password", "Password is required");
        if (password != confirmPassword)
            ModelState.AddModelError("ConfirmPassword", "Passwords do not match");
        if (string.IsNullOrWhiteSpace(role))
            ModelState.AddModelError("Role", "Role is required");

        if (ModelState.IsValid)
        {
            var employee = new Employee
            {
                FirstName = firstName,
                SecondName = secondName,
                Patronymic = patronymic,
                Email = email
            };
            var createdEmployee = await _employeeService.CreateAsync(employee);

            var user = new AppUser
            {
                UserName = email,
                Email = email,
                EmployeeId = createdEmployee.Id
            };
            var createResult = await _userManager.CreateAsync(user, password);
            if (createResult.Succeeded)
            {
                createdEmployee.AppUserId = user.Id;
                await _employeeService.UpdateAsync(createdEmployee.Id, createdEmployee);

                await _userManager.AddToRoleAsync(user, role);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                await _employeeService.DeleteAsync(createdEmployee.Id);
                foreach (var error in createResult.Errors)
                    ModelState.AddModelError("", error.Description);
            }
        }
        return View();
    }

    public async Task<IActionResult> Edit(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee == null) return NotFound();

        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.EmployeeId == id);
        var role = user != null ? (await _userManager.GetRolesAsync(user)).FirstOrDefault() : "Employee";

        var currentUser = await _userManager.GetUserAsync(User);
        bool isSelf = currentUser?.EmployeeId == id;

        ViewBag.Roles = new List<string> { "Employee", "ProjectManager" };
        ViewBag.CurrentRole = role;
        ViewBag.IsSelf = isSelf;
        return View(employee);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string firstName, string secondName, string? patronymic, string email, string? password, string? confirmPassword, string role)
    {
        ViewBag.Roles = new List<string> { "Employee", "ProjectManager" };

        var employee = await _employeeService.GetByIdAsync(id);
        if (employee == null) return NotFound();

        if (string.IsNullOrWhiteSpace(firstName))
            ModelState.AddModelError("FirstName", "First name is required");
        if (string.IsNullOrWhiteSpace(email))
            ModelState.AddModelError("Email", "Email is required");

        if (!string.IsNullOrEmpty(password) && password != confirmPassword)
            ModelState.AddModelError("ConfirmPassword", "Passwords do not match");

        if (ModelState.IsValid)
        {
            employee.FirstName = firstName;
            employee.SecondName = secondName;
            employee.Patronymic = patronymic;
            employee.Email = email;
            var updated = await _employeeService.UpdateAsync(id, employee);
            if (updated == null) return NotFound();

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.EmployeeId == id);
            if (user != null)
            {
                if (user.Email != email)
                {
                    user.UserName = email;
                    user.Email = email;
                    await _userManager.UpdateAsync(user);
                }
                if (!string.IsNullOrEmpty(password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    await _userManager.ResetPasswordAsync(user, token, password);
                }
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser?.EmployeeId != id)
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRoleAsync(user, role);
                }
            }
            return RedirectToAction(nameof(Index));
        }

        var currentUser2 = await _userManager.GetUserAsync(User);
        ViewBag.IsSelf = currentUser2?.EmployeeId == id;
        ViewBag.CurrentRole = role;
        return View(employee);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee != null)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.EmployeeId == id);
            if (user != null)
                await _userManager.DeleteAsync(user);
            await _employeeService.DeleteAsync(id);
        }
        return RedirectToAction(nameof(Index));
    }
}