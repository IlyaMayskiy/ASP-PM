namespace ASP_PM.Models;

public class Employee
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string? SecondName { get; set; }
    public string? Patronymic { get; set; }
    public string Email { get; set; } = "";
    public string? AppUserId { get; set; }
    public AppUser? AppUser { get; set; }
    public ICollection<Project> ExecutorOfProjects { get; set; } = new List<Project>();

    public string FullName => $"{SecondName} {FirstName} {Patronymic}".Trim();
}