using ASP_PM.Data;
using ASP_PM.Models;
using Microsoft.EntityFrameworkCore;

namespace ASP_PM.Services;

public class ProjectService : IProjectService
{
    private readonly AppDbContext _dbContext;
    private readonly IWebHostEnvironment _env;

    public ProjectService(AppDbContext dbContext, IWebHostEnvironment env)
    {
        _dbContext = dbContext;
        _env = env;
    }

    public async Task<Project> CreateAsync(Project project, int[] executorIds)
    {
        if (executorIds != null && executorIds.Any())
        {
            var executors = await _dbContext.Employees
                .Where(e => executorIds.Contains(e.Id))
                .ToListAsync();
            project.Executors = executors;
        }
        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync();
        return project;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var project = await _dbContext.Projects.FindAsync(id);
        if (project != null)
        {
            _dbContext.Projects.Remove(project);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<IEnumerable<Project>> GetAllAsync()
    {
        return await _dbContext.Projects
            .Include(p => p.ProjectManager)
            .Include(p => p.Executors)
            .Include(p => p.Tasks)
            .ToListAsync();
    }

    public async Task<Project?> GetByIdAsync(int id)
    {
        return await _dbContext.Projects
            .Include(p => p.ProjectManager)
            .Include(p => p.Executors)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Project>> GetFilteredAsync(DateTime? startDateFrom, DateTime? startDateTo, int? priority, string sortBy, bool ascending)
    {
        var query = _dbContext.Projects
            .Include(p => p.ProjectManager)
            .Include(p => p.Executors)
            .Include(p => p.Tasks)
            .AsQueryable();

        if (startDateFrom.HasValue)
            query = query.Where(p => p.StartDate >= startDateFrom.Value);
        if (startDateTo.HasValue)
            query = query.Where(p => p.StartDate <= startDateTo.Value);
        if (priority.HasValue)
            query = query.Where(p => p.Priority == priority.Value);

        query = sortBy?.ToLower() switch
        {
            "name" => ascending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
            "startdate" => ascending ? query.OrderBy(p => p.StartDate) : query.OrderByDescending(p => p.StartDate),
            "priority" => ascending ? query.OrderBy(p => p.Priority) : query.OrderByDescending(p => p.Priority),
            _ => query.OrderBy(p => p.Id)
        };

        return await query.ToListAsync();
    }

    public async Task<Project?> UpdateAsync(int id, Project project, int[] executorIds)
    {
        var existing = await _dbContext.Projects
            .Include(p => p.Executors)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (existing == null) return null;

        existing.Name = project.Name;
        existing.ClientCompany = project.ClientCompany;
        existing.ExecutorCompany = project.ExecutorCompany;
        existing.StartDate = project.StartDate;
        existing.EndDate = project.EndDate;
        existing.Priority = project.Priority;
        existing.ProjectManagerId = project.ProjectManagerId;

        existing.Executors.Clear();
        if (executorIds != null && executorIds.Any())
        {
            var executors = await _dbContext.Employees
                .Where(e => executorIds.Contains(e.Id))
                .ToListAsync();
            foreach (var e in executors)
                existing.Executors.Add(e);
        }

        await _dbContext.SaveChangesAsync();
        return existing;
    }

    public async Task<IEnumerable<ProjectDocument>> GetDocumentsAsync(int projectId)
    {
        return await _dbContext.ProjectDocuments
            .Where(d => d.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<ProjectDocument> AddDocumentAsync(int projectId, IFormFile file)
    {
        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        var folderPath = Path.Combine(_env.WebRootPath, "project_docs", projectId.ToString());
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var filePath = Path.Combine(folderPath, fileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
            await file.CopyToAsync(stream);

        var doc = new ProjectDocument
        {
            ProjectId = projectId,
            FileName = fileName,
            OriginalName = file.FileName,
            UploadDate = DateTime.UtcNow
        };
        _dbContext.ProjectDocuments.Add(doc);
        await _dbContext.SaveChangesAsync();
        return doc;
    }

    public async Task<ProjectDocument> AddDocumentAsync(int projectId, string fileName, string originalName)
    {
        var doc = new ProjectDocument
        {
            ProjectId = projectId,
            FileName = fileName,
            OriginalName = originalName,
            UploadDate = DateTime.UtcNow
        };
        _dbContext.ProjectDocuments.Add(doc);
        await _dbContext.SaveChangesAsync();
        return doc;
    }

    public async Task<bool> DeleteDocumentAsync(int documentId)
    {
        var doc = await _dbContext.ProjectDocuments.FindAsync(documentId);
        if (doc == null) return false;

        var folderPath = Path.Combine(_env.WebRootPath, "project_docs", doc.ProjectId.ToString());
        var filePath = Path.Combine(folderPath, doc.FileName);
        if (File.Exists(filePath))
            File.Delete(filePath);

        _dbContext.ProjectDocuments.Remove(doc);
        await _dbContext.SaveChangesAsync();
        return true;
    }
    public async Task<IEnumerable<TaskItem>> GetTasksByProjectIdAsync(int projectId)
    {
        return await _dbContext.Tasks
            .Where(t => t.ProjectId == projectId)
            .Include(t => t.Author)
            .Include(t => t.Executor)
            .ToListAsync();
    }
}