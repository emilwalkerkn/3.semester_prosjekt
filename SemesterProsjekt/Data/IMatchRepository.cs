using SemesterProsjekt.Models;
using SemesterProsjekt.Models.ViewModels;

namespace SemesterProsjekt.Data;

public interface IMatchRepository
{
    Task<IEnumerable<MatchListItemViewModel>> GetActiveAsync();
    Task<IEnumerable<MatchListItemViewModel>> GetHistoryAsync();
    Task<Match?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(int needId, int resourceId);
    Task<int> CreateAsync(Match match);
    Task UpdateStatusAsync(int id, string status, DateTime? resolvedAt);
}
