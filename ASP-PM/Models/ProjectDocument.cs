namespace ASP_PM.Models;

public class ProjectDocument
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string FileName { get; set; } = "";
    public string OriginalName { get; set; } = "";
    public DateTime UploadDate { get; set; } = DateTime.UtcNow;

    public Project? Project { get; set; }
}