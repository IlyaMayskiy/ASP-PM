using System.ComponentModel.DataAnnotations;

namespace ASP_PM.Models;

/// <summary>
/// Status of a task.
/// </summary>
public enum TaskState
{
    ToDo,
    InProgress,
    Done
}

/// <summary>
/// Represents a task within a project.
/// </summary>
public class TaskItem
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Task name (required).
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to the project.
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Navigation property to the project.
    /// </summary>
    public Project? Project { get; set; }

    /// <summary>
    /// Foreign key to the author.
    /// </summary>
    public int AuthorId { get; set; }

    /// <summary>
    /// Navigation property to the author.
    /// </summary>
    public Employee? Author { get; set; }

    /// <summary>
    /// Foreign key to the executor.
    /// </summary>
    public int? ExecutorId { get; set; }

    /// <summary>
    /// Navigation property to the executor.
    /// </summary>
    public Employee? Executor { get; set; }

    /// <summary>
    /// Status of the task.
    /// </summary>
    public TaskState Status { get; set; } = TaskState.ToDo;

    /// <summary>
    /// Comment or description.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Integer priority (higher value = higher priority).
    /// </summary>
    public int Priority { get; set; } = 1;
}