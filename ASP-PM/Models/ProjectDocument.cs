namespace ASP_PM.Models;

/// <summary>
/// Represents a document uploaded for a project.
/// </summary>
public class ProjectDocument
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the project.
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Server file name (GUID + extension).
    /// </summary>
    public string FileName { get; set; } = "";

    /// <summary>
    /// Original file name as uploaded by the user.
    /// </summary>
    public string OriginalName { get; set; } = "";

    /// <summary>
    /// Upload date and time.
    /// </summary>
    public DateTime UploadDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property to the project.
    /// </summary>
    public Project? Project { get; set; }
}