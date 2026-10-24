using SemesterProsjekt.Models;

namespace SemesterProsjekt.Data;

public interface INeedRepository
{
    Task<IEnumerable<Need>> GetAllAsync();

    Task<Need?> GetByIdAsync(int id);

    Task<int> CreateAsync(Need need);

    Task<bool> UpdateStatusAsync(int id, int userId, bool canManageAll, string status);

    Task<IEnumerable<NeedType>> GetNeedTypesAsync();
}
