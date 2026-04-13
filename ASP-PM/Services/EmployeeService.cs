using ASP_PM.Data;
using ASP_PM.Models;
using Microsoft.EntityFrameworkCore;

namespace ASP_PM.Services;

public class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _dbContext;

    public EmployeeService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Employee> CreateAsync(Employee employee)
    {
        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();
        return employee;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var employee = await _dbContext.Employees.FindAsync(id);
        if (employee == null) return false;
        _dbContext.Employees.Remove(employee);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Employee>> GetAllAsync()
    {
        return await _dbContext.Employees.ToListAsync();
    }

    public async Task<Employee?> GetByIdAsync(int id)
    {
        return await _dbContext.Employees.FindAsync(id);
    }

    public async Task<Employee?> UpdateAsync(int id, Employee employee)
    {
        var existing = await _dbContext.Employees.FindAsync(id);
        if (existing == null) return null;
        existing.FirstName = employee.FirstName;
        existing.SecondName = employee.SecondName;
        existing.Patronymic = employee.Patronymic;
        existing.Email = employee.Email;
        await _dbContext.SaveChangesAsync();
        return existing;
    }

    public async Task<Employee?> GetByAppUserIdAsync(string appUserId)
    {
        return await _dbContext.Employees.FirstOrDefaultAsync(e => e.AppUserId == appUserId);
    }
}