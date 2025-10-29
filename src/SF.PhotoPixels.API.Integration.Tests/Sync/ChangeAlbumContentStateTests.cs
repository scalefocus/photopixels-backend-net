using FluentAssertions;
using SF.PhotoPixels.Application.Query.StateChanges;
using SF.PhotoPixels.Domain.Events;
using Xunit;

namespace SF.PhotoPixels.API.Integration.Tests.Sync;

public class ChangeAlbumContentStateTests
{
    private static AlbumStateChangesResponseDetails NewState()
    {
        return new AlbumStateChangesResponseDetails
        {
            Id = Guid.NewGuid(),
            Version = 0
        };
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Apply_ObjectToAlbumCreated_ShouldAppearInAdded()
    {
        var state = NewState();

        var photoId = "photo-1";
        var addedAt = DateTimeOffset.UtcNow;

        state.Apply(new ObjectToAlbumCreated
        {
            ObjectId = photoId,
            AddedAt = addedAt,
            AlbumId = default,
        });

        state.Added.Should().ContainKey(photoId);
        state.Removed.Should().NotContain(photoId);

        state.Added[photoId].Should().Be(addedAt.ToUnixTimeMilliseconds());
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Apply_ObjectToAlbumDeleted_ShouldAppearInRemoved()
    {
        var state = NewState();

        var photoId = "photo-2";

        state.Apply(new ObjectToAlbumDeleted
        {
            ObjectId = photoId,
            RemovedAt = DateTimeOffset.UtcNow,
            AlbumId = default
        });

        state.Removed.Should().Contain(photoId);
        state.Added.Should().NotContainKey(photoId);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Apply_ObjectAddedThenRemoved_ShouldDisappearFromBoth()
    {
        var state = NewState();

        var photoId = "photo-3";
        var t1 = DateTimeOffset.UtcNow;
        var t2 = t1.AddSeconds(5);

        state.Apply(new ObjectToAlbumCreated
        {
            ObjectId = photoId,
            AddedAt = t1,
            AlbumId = default
        });

        state.Apply(new ObjectToAlbumDeleted
        {
            ObjectId = photoId,
            RemovedAt = t2,
            AlbumId = default
        });

        state.Added.Should().NotContainKey(photoId);
        state.Removed.Should().NotContain(photoId);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Apply_ObjectRemovedThenAdded_ShouldEndUpOnlyInAdded()
    {
        var state = NewState();

        var photoId = "photo-4";
        var removedAt = DateTimeOffset.UtcNow;
        var addedAgainAt = removedAt.AddMinutes(1);

        state.Apply(new ObjectToAlbumDeleted
        {
            ObjectId = photoId,
            RemovedAt = removedAt,
            AlbumId = default
        });

        state.Apply(new ObjectToAlbumCreated
        {
            ObjectId = photoId,
            AddedAt = addedAgainAt,
            AlbumId = default
        });

        state.Removed.Should().NotContain(photoId);
        state.Added.Should().ContainKey(photoId);
        state.Added[photoId].Should().Be(addedAgainAt.ToUnixTimeMilliseconds());
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Apply_ObjectAddedTwice_ShouldKeepLatestTimestamp()
    {
        var state = NewState();

        var photoId = "photo-5";
        var firstTime = DateTimeOffset.UtcNow;
        var secondTime = firstTime.AddMinutes(10);

        state.Apply(new ObjectToAlbumCreated
        {
            ObjectId = photoId,
            AddedAt = firstTime,
            AlbumId = default
        });

        state.Apply(new ObjectToAlbumCreated
        {
            ObjectId = photoId,
            AddedAt = secondTime,
            AlbumId = default
        });

        state.Added.Should().ContainKey(photoId);
        state.Added[photoId].Should().Be(secondTime.ToUnixTimeMilliseconds());

        state.Removed.Should().NotContain(photoId);
    }
}
