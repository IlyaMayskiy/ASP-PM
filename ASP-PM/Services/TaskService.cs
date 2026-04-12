using ASP_PM.Data;
using ASP_PM.Models;
using Microsoft.EntityFrameworkCore;

namespace ASP_PM.Services;

public class TaskService : ITaskService
{
    private readonly AppDbContext _context;

    public TaskService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TaskItem>> GetAllAsync()
    {
        return await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.Author)
            .Include(t => t.Executor)
            .ToListAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(int id)
    {
        return await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.Author)
            .Include(t => t.Executor)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<TaskItem> CreateAsync(TaskItem task)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<TaskItem?> UpdateAsync(int id, TaskItem task)
    {
        var existing = await _context.Tasks.FindAsync(id);
        if (existing == null) return null;

        existing.Name = task.Name;
        existing.ProjectId = task.ProjectId;
        existing.AuthorId = task.AuthorId;
        existing.ExecutorId = task.ExecutorId;
        existing.Status = task.Status;
        existing.Comment = task.Comment;
        existing.Priority = task.Priority;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return false;
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<TaskItem>> GetFilteredAsync(int? projectId, TaskState? status, string sortBy, bool ascending)
    {
        var query = _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.Author)
            .Include(t => t.Executor)
            .AsQueryable();

        if (projectId.HasValue)
            query = query.Where(t => t.ProjectId == projectId.Value);
        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        query = sortBy?.ToLower() switch
        {
            "name" => ascending ? query.OrderBy(t => t.Name) : query.OrderByDescending(t => t.Name),
            "priority" => ascending ? query.OrderBy(t => t.Priority) : query.OrderByDescending(t => t.Priority),
            "status" => ascending ? query.OrderBy(t => t.Status) : query.OrderByDescending(t => t.Status),
            _ => query.OrderBy(t => t.Id)
        };

        return await query.ToListAsync();
    }
}