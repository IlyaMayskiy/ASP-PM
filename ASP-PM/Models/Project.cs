namespace ASP_PM.Models;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ClientCompany { get; set; } = string.Empty;
    public string ExecutorCompany { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Priority { get; set; }
    public int? ProjectManagerId { get; set; }
    public Employee? ProjectManager { get; set; }
    public ICollection<Employee> Executors { get; set; } = new List<Employee>();

}
