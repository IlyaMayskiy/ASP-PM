using System.ComponentModel.DataAnnotations;

namespace ASP_PM.Models;

public enum TaskState
{
    ToDo,
    InProgress,
    Done
}

public class TaskItem
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public int ProjectId { get; set; }
    public Project? Project { get; set; }

    public int AuthorId { get; set; }
    public Employee? Author { get; set; }

    public int? ExecutorId { get; set; }
    public Employee? Executor { get; set; }

    public TaskState Status { get; set; } = TaskState.ToDo;

    public string? Comment { get; set; }

    public int Priority { get; set; } = 1;
}