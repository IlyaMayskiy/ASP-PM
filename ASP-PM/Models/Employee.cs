namespace ASP_PM.Models;

public class Employee
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? SecondName { get; set; }
    public string? Patronymic { get; set; }
    public string Email { get; set; } = string.Empty;

    public ICollection<Project> ExecutorOfProjects { get; set; } = new List<Project>();
}
