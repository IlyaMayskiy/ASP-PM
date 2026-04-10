namespace ASP_PM.Models;

public class ProjectWizardModel
{
    public string Name { get; set; } = "";
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(1);
    public int Priority { get; set; } = 1;

    public string ClientCompany { get; set; } = "";
    public string ExecutorCompany { get; set; } = "";

    public int? ProjectManagerId { get; set; }

    public int[] SelectedExecutors { get; set; } = Array.Empty<int>();

}