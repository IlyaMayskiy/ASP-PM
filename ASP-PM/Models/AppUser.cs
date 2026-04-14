using Microsoft.AspNetCore.Identity;

namespace ASP_PM.Models;

/// <summary>
/// Application user extending IdentityUser.
/// </summary>
public class AppUser : IdentityUser
{
    /// <summary>
    /// Foreign key to the associated employee (nullable).
    /// </summary>
    public int? EmployeeId { get; set; }

    /// <summary>
    /// Navigation property to the employee.
    /// </summary>
    public Employee? Employee { get; set; }
}