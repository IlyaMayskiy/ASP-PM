using ASP_PM.Models;
using ASP_PM.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASP_PM.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // GET: Employees
        public async Task<IActionResult> Index()
        {
            var employees = await _employeeService.GetAllAsync();
            return View(employees);
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
            if (ModelState.IsValid)
            {
                await _employeeService.CreateAsync(employee);
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _employeeService.GetByIdAsync(id);
            if (employee == null)
                return NotFound();
            return View(employee);
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee)
        {
            if (id != employee.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                var updated = await _employeeService.UpdateAsync(id, employee);
                if (updated == null)
                    return NotFound();
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _employeeService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}