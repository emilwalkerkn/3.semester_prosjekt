using SemesterProsjekt.Models;

namespace SemesterProsjekt.Data;

public interface IAccountRepository
{
    Task<ProfileSettingsViewModel?> GetProfileAsync(int userId);

    Task UpdateProfileAsync(int userId, ProfileSettingsViewModel profile);

    Task<string?> GetPasswordHashAsync(int userId);

    Task UpdatePasswordHashAsync(int userId, string passwordHash);

    Task<IEnumerable<string>> GetRoleNamesAsync(int userId);
}
