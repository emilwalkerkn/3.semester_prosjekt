namespace SemesterProsjekt.Models;

public class DashboardViewModel
{
    public int TotalNeeds { get; set; }
    public int TotalResources { get; set; }
    
    public IEnumerable<Need> RecentNeeds { get; set; } = new List<Need>();
    
    public IEnumerable<Resource> RecentResources { get; set; } = new List<Resource>();
}