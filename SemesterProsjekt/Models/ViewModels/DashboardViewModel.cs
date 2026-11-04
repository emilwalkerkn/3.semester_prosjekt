namespace SemesterProsjekt.Models;

/// <summary>
/// Data til forsiden (dashboardet): totalt antall behov og ressurser,
/// pluss de nyeste registreringene. Fylles i HomeController.Index.
/// </summary>
public class DashboardViewModel
{
    // Antall registrerte behov og ressurser totalt.
    public int TotalNeeds { get; set; }
    public int TotalResources { get; set; }

    // De sist registrerte behovene (de tre med høyest ID).
    public IEnumerable<Need> RecentNeeds { get; set; } = new List<Need>();

    // De sist registrerte ressursene (de tre med høyest ID).
    public IEnumerable<Resource> RecentResources { get; set; } = new List<Resource>();
}
