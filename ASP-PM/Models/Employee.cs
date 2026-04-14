namespace ASP_PM.Models;

/// <summary>
/// Represents an employee in the system.
/// </summary>
public class Employee
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// First name (required).
    /// </summary>
    public string FirstName { get; set; } = "";
    
    /// <summary>
    /// Last name (optional).
    /// </summary>
    public string? SecondName { get; set; }

    /// <summary>
    /// Patronymic name (optional).
    /// </summary>
    public string? Patronymic { get; set; }

    /// <summary>
    /// Email address (required, used as login).
    /// </summary>
    public string Email { get; set; } = "";

    /// <summary>
    /// Foreign key to AspNetUsers table (Identity user).
    /// </summary>
    public string? AppUserId { get; set; }

    /// <summary>
    /// Navigation property to the Identity user.
    /// </summary>
    public AppUser? AppUser { get; set; }

    /// <summary>
    /// Projects where this employee is an executor (many-to-many).
    /// </summary>
    public ICollection<Project> ExecutorOfProjects { get; set; } = new List<Project>();

    /// <summary>
    /// Full name combining last name, first name and patronymic.
    /// </summary>
    public string FullName => $"{SecondName} {FirstName} {Patronymic}".Trim();
}