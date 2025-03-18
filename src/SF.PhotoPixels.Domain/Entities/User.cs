using System.Security.Principal;
using Marten.Metadata;
using Marten.Schema;
using SF.PhotoPixels.Domain.Enums;

namespace SF.PhotoPixels.Domain.Entities;

public class User : IIdentity, ISoftDeleted
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Email { get; set; }

    public string NormalizedEmail { get; set; } = string.Empty;

    public bool EmailConfirmed { get; set; } = false;

    public string? PasswordHash { get; set; }

    public required string UserName { get; set; }

    public string NormalizedUserName { get; set; } = string.Empty;

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public Role Role { get; set; } = Role.Contributor;

    public long Quota { get; set; } = 50L * 1024 * 1024 * 1024;

    public long UsedQuota { get; set; }

    public required string Name { get; set; }

    public bool IsAuthenticated { get; set; } = false;

    public string AuthenticationType { get; set; } = string.Empty;

    public string? SecurityStamp { get; set; }

    public bool Deleted { get; set; }

    [SoftDeletedAtMetadata]
    public DateTimeOffset? DeletedAt { get; set; }

    public bool IncreaseUsedQuota(long size)
    {
        if (UsedQuota + size > Quota)
        {
            return false;
        }

        UsedQuota += size;

        return true;
    }

    public void DecreaseUsedQuota(long size)
    {
        UsedQuota -= size;
    }
}