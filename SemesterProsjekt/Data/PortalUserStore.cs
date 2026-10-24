using Dapper;
using Microsoft.AspNetCore.Identity;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.Data;

public class PortalUserStore : IUserStore<PortalUser>, IUserPasswordStore<PortalUser>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PortalUserStore(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public void Dispose() { }

    public Task<string> GetUserIdAsync(PortalUser user, CancellationToken cancellationToken) =>
        Task.FromResult(user.Id.ToString());

    public Task<string?> GetUserNameAsync(PortalUser user, CancellationToken cancellationToken) =>
        Task.FromResult(user.UserName);

    public Task SetUserNameAsync(PortalUser user, string? userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(PortalUser user, CancellationToken cancellationToken) =>
        Task.FromResult(user.NormalizedUserName);

    public Task SetNormalizedUserNameAsync(PortalUser user, string? normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public async Task<PortalUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        if (!int.TryParse(userId, out var id)) return null;
        return await FindAsync("UserId = @Id", new { Id = id });
    }

    public Task<PortalUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) =>
        FindAsync("UPPER(Username) = @Username", new { Username = normalizedUserName });

    public Task SetPasswordHashAsync(PortalUser user, string? passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(PortalUser user, CancellationToken cancellationToken) =>
        Task.FromResult(user.PasswordHash);

    public Task<bool> HasPasswordAsync(PortalUser user, CancellationToken cancellationToken) =>
        Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));

    public async Task<IdentityResult> UpdateAsync(PortalUser user, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("""
            UPDATE Users SET Username = @Username, Email = @Email, FirstName = @FirstName,
            LastName = @LastName, Phone = @PhoneNumber, PasswordHash = @PasswordHash
            WHERE UserId = @Id;
            """, user);
        return IdentityResult.Success;
    }

    public Task<IdentityResult> CreateAsync(PortalUser user, CancellationToken cancellationToken) =>
        Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "Brukerregistrering er ikke tilgjengelig ennå." }));

    public Task<IdentityResult> DeleteAsync(PortalUser user, CancellationToken cancellationToken) =>
        Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "Brukersletting er ikke tilgjengelig." }));

    private async Task<PortalUser?> FindAsync(string whereClause, object parameters)
    {
        using var connection = _connectionFactory.CreateConnection();
        var user = await connection.QuerySingleOrDefaultAsync<PortalUser>($"""
            SELECT UserId AS Id, Username AS UserName, UPPER(Username) AS NormalizedUserName,
                   Email, FirstName, LastName, Phone AS PhoneNumber, PasswordHash
            FROM Users WHERE {whereClause};
            """, parameters);
        return user;
    }
}
