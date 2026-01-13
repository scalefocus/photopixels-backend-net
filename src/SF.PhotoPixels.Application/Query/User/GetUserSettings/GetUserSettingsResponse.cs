using SF.PhotoPixels.Domain.Entities;

namespace SF.PhotoPixels.Application.Query.User.GetUserSettings
{
    public class GetUserSettingsResponse
    {
        public required Guid UserId { get; set; }

        public UserSettings Settings { get; set; }
    }
}
