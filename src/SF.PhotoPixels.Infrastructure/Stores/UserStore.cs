using Marten;
using Microsoft.AspNetCore.Identity;
using SF.PhotoPixels.Domain.Entities;

namespace SF.PhotoPixels.Infrastructure.Stores;

public class UserStore : IUserPasswordStore<User>,
    IUserEmailStore<User>,
    IUserSecurityStampStore<User>,
    IQueryableUserStore<User>
{
    private readonly IDocumentSession _session;

    public UserStore(IDocumentSession documentSession)
    {
        _session = documentSession;
    }

    public void Dispose()
    {
    }

    #region crud user

    public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        _session.Store(user);
        await _session.SaveChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        _session.Update(user);
        await _session.SaveChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }


        _session.Delete(user);
        await _session.SaveChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }

    #endregion

    #region user Id

    public async Task<User?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (userId == null)
        {
            throw new ArgumentNullException(nameof(userId));
        }

        if (!Guid.TryParse(userId, out var idGuid))
        {
            throw new ArgumentException("Not a valid Guid id", nameof(userId));
        }

        var user = await _session.Query<User>()
            .SingleOrDefaultAsync(x => x.Id == idGuid, cancellationToken);

        return user;
    }

    public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return Task.FromResult(user.Id.ToString());
    }

    #endregion

    #region user email

    public async Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (normalizedEmail == null)
        {
            throw new ArgumentNullException(nameof(normalizedEmail));
        }

        var user = await _session.Query<User>()
            .SingleOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);

        return user;
    }

    public Task<string?> GetEmailAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return Task.FromResult<string?>(user.Email);
    }

    public Task<bool> GetEmailConfirmedAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return Task.FromResult(user.EmailConfirmed);
    }

    public Task<string?> GetNormalizedEmailAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return Task.FromResult<string?>(user.NormalizedEmail);
    }

    public Task SetEmailAsync(User user, string? email, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (email == null)
        {
            throw new ArgumentNullException(nameof(email));
        }

        user.Email = email;

        return Task.CompletedTask;
    }

    public Task SetEmailConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        user.EmailConfirmed = confirmed;

        return Task.CompletedTask;
    }

    public Task SetNormalizedEmailAsync(User user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (normalizedEmail == null)
        {
            throw new ArgumentNullException(nameof(normalizedEmail));
        }

        user.NormalizedEmail = normalizedEmail;

        return Task.CompletedTask;
    }

    #endregion

    #region username

    public async Task<User?> FindByNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (userName == null)
        {
            throw new ArgumentNullException(nameof(userName));
        }

        var user = await _session.Query<User>()
            .SingleOrDefaultAsync(x => x.NormalizedUserName == userName, cancellationToken);

        return user;
    }

    public Task<string?> GetUserNameAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return Task.FromResult<string?>(user.UserName);
    }

    public Task<string?> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return Task.FromResult<string?>(user.NormalizedUserName);
    }

    public Task SetUserNameAsync(User user, string? userName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (userName == null)
        {
            throw new ArgumentNullException(nameof(userName));
        }

        user.UserName = userName;

        return Task.CompletedTask;
    }


    public Task SetNormalizedUserNameAsync(User user, string? normalizedName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (normalizedName == null)
        {
            throw new ArgumentNullException(nameof(normalizedName));
        }

        user.NormalizedUserName = normalizedName;

        return Task.CompletedTask;
    }

    #endregion

    #region user password

    public Task<string?> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return Task.FromResult(user.PasswordHash);
    }

    public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return Task.FromResult(user.PasswordHash != null);
    }

    public Task SetPasswordHashAsync(User user, string? passwordHash, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (passwordHash == null)
        {
            throw new ArgumentNullException(nameof(passwordHash));
        }

        user.PasswordHash = passwordHash;

        return Task.CompletedTask;
    }

    public Task<string?> GetSecurityStampAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return Task.FromResult(user.SecurityStamp);
    }

    public Task SetSecurityStampAsync(User user, string stamp, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (stamp == null)
        {
            throw new ArgumentNullException(nameof(stamp));
        }

        user.SecurityStamp = stamp;

        return Task.CompletedTask;
    }

    #endregion

    public IQueryable<User> Users => _session.Query<User>();
}