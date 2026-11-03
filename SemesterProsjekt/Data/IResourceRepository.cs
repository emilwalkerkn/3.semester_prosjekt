using SemesterProsjekt.Models;

namespace SemesterProsjekt.Data;

public interface IResourceRepository
{
    Task<IEnumerable<Resource>> GetAllAsync();

    Task<Resource?> GetByIdAsync(int id);

    Task<int> CreateAsync(Resource resource);

    Task UpdateAsync(Resource resource);

    Task DeleteAsync(int id);

    Task<IEnumerable<ResourceType>> GetResourceTypesAsync();
}
