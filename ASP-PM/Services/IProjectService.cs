using ASP_PM.Models;

namespace ASP_PM.Services;

public interface IProjectService
{
    Task<IEnumerable<Project>> GetAllAsync();
    Task<Project?> GetByIdAsync(int id);
    Task<Project> CreateAsync(Project project, int[] executorIds);
    Task<Project?> UpdateAsync(int id, Project project, int[] executorIds);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<Project>> GetFilteredAsync(DateTime? startDateFrom, DateTime? startDateTo,int? priority, string sortBy, bool ascending);
}
