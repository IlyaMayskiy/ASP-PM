using System.ComponentModel.DataAnnotations;

namespace ASP_PM.Models;

/// <summary>
/// Represents a project in the system.
/// </summary>
public class Project
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Project name (required).
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Company name.
    /// </summary>
    public string ClientCompany { get; set; } = string.Empty;

    /// <summary>
    /// Executor company name.
    /// </summary>
    public string ExecutorCompany { get; set; } = string.Empty;

    /// <summary>
    /// Project start date.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Project end date.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Integer priority (higher value = higher priority).
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Foreign key to the project manager (Employee).
    /// </summary>
    public int? ProjectManagerId { get; set; }

    /// <summary>
    /// Navigation property to the project manager.
    /// </summary>
    public Employee? ProjectManager { get; set; }

    /// <summary>
    /// List of employees assigned as executors (many-to-many).
    /// </summary>
    public ICollection<Employee> Executors { get; set; } = new List<Employee>();

    /// <summary>
    /// List of tasks belonging to this project (one-to-many).
    /// </summary>
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}