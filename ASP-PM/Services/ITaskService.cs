using ASP_PM.Models;

namespace ASP_PM.Services;

public interface ITaskService
{
    Task<IEnumerable<TaskItem>> GetAllAsync();
    Task<TaskItem?> GetByIdAsync(int id);
    Task<TaskItem> CreateAsync(TaskItem task);
    Task<TaskItem?> UpdateAsync(int id, TaskItem task);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<TaskItem>> GetFilteredAsync(int? projectId, TaskState? status, string sortBy, bool ascending);
}