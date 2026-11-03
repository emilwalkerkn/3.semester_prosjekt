using SemesterProsjekt.Models;

namespace SemesterProsjekt.Data;

public interface INeedRepository
{
    Task<IEnumerable<Need>> GetAllAsync(string sortBy = "newest");

    Task<Need?> GetByIdAsync(int id);

    Task<int> CreateAsync(Need need);

    Task UpdateAsync(Need need);

    Task DeleteAsync(int id);

    Task UpdateStatusAsync(int id, string status);

    Task<IEnumerable<NeedType>> GetNeedTypesAsync();
}
