using SemesterProsjekt.Models;

namespace SemesterProsjekt.Data;

public interface IMatchRepository
{
    Task<IEnumerable<ResourceMatch>> GetVisibleAsync(int userId, bool canManageAll);
    Task<IEnumerable<Resource>> GetAvailableResourcesAsync();
    Task<int?> CreateAsync(int needId, int resourceId, decimal assignedQuantity, int userId, bool canManageAll);
    Task<bool> UpdateStatusAsync(int matchId, int userId, bool canManageAll, string status);
}
