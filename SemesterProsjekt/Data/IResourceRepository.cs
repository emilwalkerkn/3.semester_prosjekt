using SemesterProsjekt.Models;

namespace SemesterProsjekt.Data;

public interface IResourceRepository
{
    Task<IEnumerable<Resource>> GetAllAsync();

    Task<Resource?> GetByIdAsync(int id);

    Task<int> CreateAsync(Resource resource);

    Task<bool> UpdateAsync(Resource resource, int userId, bool canManageAll);

    Task<bool> DeleteAsync(int resourceId, int userId, bool canManageAll);
}
