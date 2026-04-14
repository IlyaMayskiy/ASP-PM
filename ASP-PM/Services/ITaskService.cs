using ASP_PM.Models;

namespace ASP_PM.Services;

/// <summary>
/// Everything you can do with a task – create, edit, delete, filter, and sort.
/// </summary>
public interface ITaskService
{
    /// <summary>All tasks, together with their project, author, and executor.</summary>
    Task<IEnumerable<TaskItem>> GetAllAsync();

    /// <summary>Get a single task by its ID, with all related data.</summary>
    Task<TaskItem?> GetByIdAsync(int id);

    /// <summary>Add a new task to some project.</summary>
    Task<TaskItem> CreateAsync(TaskItem task);

    /// <summary>Change task's name, assignee, status, priority, or comment.</summary>
    Task<TaskItem?> UpdateAsync(int id, TaskItem task);

    /// <summary>Permanently remove a task from the system.</summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>Filter tasks by project and/or status, then order by name, priority, or status.</summary>
    Task<IEnumerable<TaskItem>> GetFilteredAsync(int? projectId, TaskState? status, string sortBy, bool ascending);
}