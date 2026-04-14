namespace ASP_PM.Models;

/// <summary>
/// View model for the project creation wizard.
/// </summary>
public class ProjectWizardModel
{
    /// <summary>
    /// Project name.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Project start date.
    /// </summary>
    public DateTime StartDate { get; set; } = DateTime.Today;

    /// <summary>
    /// Project end date.
    /// </summary>
    public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(1);

    /// <summary>
    /// Integer priority.
    /// </summary>
    public int Priority { get; set; } = 1;

    /// <summary>
    /// Customer company name.
    /// </summary>
    public string ClientCompany { get; set; } = "";

    /// <summary>
    /// Executor company name.
    /// </summary>
    public string ExecutorCompany { get; set; } = "";

    /// <summary>
    /// ID of the selected project manager.
    /// </summary>
    public int? ProjectManagerId { get; set; }

    /// <summary>
    /// Array of selected executor IDs.
    /// </summary>
    public int[] SelectedExecutors { get; set; } = Array.Empty<int>();

}