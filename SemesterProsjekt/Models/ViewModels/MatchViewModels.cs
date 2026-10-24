using System.ComponentModel.DataAnnotations;

namespace SemesterProsjekt.Models.ViewModels;

public class MatchCreateViewModel
{
    public Need Need { get; set; } = new();
    public IEnumerable<Resource> AvailableResources { get; set; } = [];

    [Required(ErrorMessage = "Velg en ressurs før du foreslår en match.")]
    [Display(Name = "Ressurs")]
    public int? ResourceId { get; set; }

    [Range(typeof(decimal), "0.01", "999999999", ErrorMessage = "Mengden må være større enn null.")]
    [Display(Name = "Mengde som skal tildeles")]
    public decimal AssignedQuantity { get; set; }
}

public class MatchOverviewViewModel
{
    public IEnumerable<ResourceMatch> Matches { get; set; } = [];
    public int CurrentUserId { get; set; }
    public bool CanManageAll { get; set; }
}
