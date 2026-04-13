using Microsoft.AspNetCore.Identity;

namespace ASP_PM.Models;

public class AppUser : IdentityUser
{
    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
}