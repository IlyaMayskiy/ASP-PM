using ASP_PM.Models;
using Microsoft.AspNetCore.Http;

namespace ASP_PM.Services;

/// <summary>
/// Heavy lifter for projects: CRUD, filtering, documents, and tasks inside projects.
/// </summary>
public interface IProjectService
{
    /// <summary>Grabs all projects with their managers, executors, and attached tasks.</summary>
    Task<IEnumerable<Project>> GetAllAsync();

    /// <summary>Fetches a single project by ID, eager‑loading everything you might need.</summary>
    Task<Project?> GetByIdAsync(int id);

    /// <summary>Creates a brand‑new project and assigns a bunch of executors in one go.</summary>
    Task<Project> CreateAsync(Project project, int[] executorIds);

    /// <summary>Updates project details and replaces the executor list entirely.</summary>
    Task<Project?> UpdateAsync(int id, Project project, int[] executorIds);

    /// <summary>Deletes a project – all its tasks will be gone as well (cascade).</summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>Deletes a project – all its tasks will be gone as well (cascade).</summary>
    Task<IEnumerable<Project>> GetFilteredAsync(DateTime? startDateFrom, DateTime? startDateTo, int? priority, string sortBy, bool ascending);

    /// <summary>Projects where a given employee is the manager (used for role‑based views).</summary>
    Task<IEnumerable<Project>> GetProjectsByManagerIdAsync(int managerId, DateTime? startDateFrom, DateTime? startDateTo, int? priority, string sortBy, bool ascending);

    /// <summary>Projects where a given employee is among the executors.</summary>
    Task<IEnumerable<Project>> GetProjectsByExecutorIdAsync(int executorId, DateTime? startDateFrom, DateTime? startDateTo, int? priority, string sortBy, bool ascending);

    /// <summary>Lists all uploaded documents for a project.</summary>
    Task<IEnumerable<ProjectDocument>> GetDocumentsAsync(int projectId);

    /// <summary>Saves a file from the wizard/editor and creates a document record.</summary>
    Task<ProjectDocument> AddDocumentAsync(int projectId, IFormFile file);

    /// <summary>Adds a document record for a file that already lives on disk (e.g., moved from temp).</summary>
    Task<ProjectDocument> AddDocumentAsync(int projectId, string fileName, string originalName);

    /// <summary>Deletes the physical file and its database entry.</summary>
    Task<bool> DeleteDocumentAsync(int documentId);

    /// <summary>All tasks that belong to a specific project (with author and executor loaded).</summary>
    Task<IEnumerable<TaskItem>> GetTasksByProjectIdAsync(int projectId);
}