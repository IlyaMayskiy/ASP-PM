using ASP_PM.Models;

namespace ASP_PM.Services;

public interface IEmployeeService
{
    Task<IEnumerable<Employee>> GetAllAsync();
    Task<Employee?> GetByIdAsync(int id);
    Task<Employee> CreateAsync(Employee employee);
    Task<Employee?> UpdateAsync(int id, Employee employee);
    Task<bool> DeleteAsync(int id);
}
