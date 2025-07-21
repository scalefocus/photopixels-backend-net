using System.ComponentModel;

namespace SF.PhotoPixels.Domain.Enums;

public enum FavoriteActionType
{
    [Description("Unknown")]
    Unknown,
    [Description("Add to Favorites")]
    AddToFavorites,
    [Description("Remove From Favorites")]
    RemoveFromFavorites,
}