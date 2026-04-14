using ASP_PM.Models;

namespace ASP_PM.Services;

/// <summary>
/// All things employees: fetch, create, update, delete, and link to Identity.
/// </summary>
public interface IEmployeeService
{
    /// <summary>Returns everyone in the company (no filters).</summary>
    Task<IEnumerable<Employee>> GetAllAsync();

    /// <summary>Finds a single employee by their database ID. May return null.</summary>
    Task<Employee?> GetByIdAsync(int id);

    /// <summary>Adds a new worker to the system.</summary>
    Task<Employee> CreateAsync(Employee employee);

    /// <summary>Updates employee's personal info (name, email, etc.).</summary>
    Task<Employee?> UpdateAsync(int id, Employee employee);

    /// <summary>Removes an employee from the company. Irreversible.</summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>Looks up an employee by their linked Identity user ID.</summary>
    Task<Employee?> GetByAppUserIdAsync(string appUserId);
}
