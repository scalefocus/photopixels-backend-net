using FluentAssertions;
using SF.PhotoPixels.Application.Query.StateChanges;
using SF.PhotoPixels.Domain.Events;
using Xunit;

namespace SF.PhotoPixels.API.Integration.Tests.Sync;

public class ChangeAlbumsStateTests
{
    private static StateChangesResponseDetails NewState()
    {
        return new StateChangesResponseDetails
        {
            Id = Guid.NewGuid(),
            Version = 0
        };
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Apply_AlbumCreated_ShouldAppearInAdded()
    {
        var state = NewState();

        var albumId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;

        state.Apply(new AlbumCreated
        {
            AlbumId = albumId,
            Name = "Test Album",
            CreatedAt = createdAt,
            IsSystem = false,
            UserId = Guid.NewGuid()
        });

        state.Albums.Added.Should().ContainKey(albumId.ToString());
        state.Albums.Updated.Should().BeEmpty();
        state.Albums.Deleted.Should().BeEmpty();

        var ts = state.Albums.Added[albumId.ToString()];
        ts.Should().Be(createdAt.ToUnixTimeMilliseconds());
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Apply_AlbumCreatedThenDeleted_ShouldNotAppearAnywhere()
    {
        var state = NewState();

        var albumId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;
        var deletedAt = createdAt.AddMinutes(1);

        state.Apply(new AlbumCreated
        {
            AlbumId = albumId,
            Name = "Temp Album",
            CreatedAt = createdAt,
            IsSystem = false,
            UserId = Guid.NewGuid()
        });

        state.Apply(new AlbumDeleted
        {
            AlbumId = albumId,
            DeletedAt = deletedAt,
            UserId = Guid.NewGuid()
        });

        state.Albums.Added.Should().NotContainKey(albumId.ToString());
        state.Albums.Deleted.Should().NotContain(albumId.ToString());
        state.Albums.Updated.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Apply_AlbumCreatedThenUpdated_ShouldStayInAddedWithLatestTimestamp()
    {
        var state = NewState();

        var albumId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;
        var updatedAt = createdAt.AddMinutes(5);

        state.Apply(new AlbumCreated
        {
            AlbumId = albumId,
            Name = "Renamable Album",
            CreatedAt = createdAt,
            IsSystem = false,
            UserId = Guid.NewGuid()
        });

        state.Apply(new AlbumUpdated
        {
            AlbumId = albumId,
            UpdatedAt = updatedAt,
            UserId = Guid.NewGuid(),
            Name = "Renamed"
        });

        state.Albums.Added.Should().ContainKey(albumId.ToString());
        state.Albums.Updated.Should().BeEmpty();
        state.Albums.Deleted.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Apply_AlbumUpdatedExistingAlbum_ShouldAppearInUpdated()
    {
        var state = NewState();

        var albumId = Guid.NewGuid();
        var updatedAt = DateTimeOffset.UtcNow;

        state.Apply(new AlbumUpdated
        {
            AlbumId = albumId,
            UpdatedAt = updatedAt,
            UserId = Guid.NewGuid(),
            Name = "Changed Name"
        });

        state.Albums.Updated.Should().ContainKey(albumId.ToString());
        state.Albums.Added.Should().BeEmpty();
        state.Albums.Deleted.Should().BeEmpty();

        var ts = state.Albums.Updated[albumId.ToString()];
        ts.Should().Be(updatedAt.ToUnixTimeMilliseconds());
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Apply_AlbumUpdatedThenDeleted_ShouldEndUpOnlyInDeleted()
    {
        var state = NewState();

        var albumId = Guid.NewGuid();
        var updatedAt = DateTimeOffset.UtcNow;
        var deletedAt = updatedAt.AddMinutes(10);

        state.Apply(new AlbumUpdated
        {
            AlbumId = albumId,
            UpdatedAt = updatedAt,
            UserId = Guid.NewGuid(),
            Name = "Changed Again"
        });

        state.Apply(new AlbumDeleted
        {
            AlbumId = albumId,
            DeletedAt = deletedAt,
            UserId = Guid.NewGuid()
        });

        state.Albums.Deleted.Should().Contain(albumId.ToString());
        state.Albums.Updated.Should().NotContainKey(albumId.ToString());
        state.Albums.Added.Should().BeEmpty();
    }
}
