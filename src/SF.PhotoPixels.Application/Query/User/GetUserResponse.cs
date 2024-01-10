using SF.PhotoPixels.Domain.Enums;

namespace SF.PhotoPixels.Application.Query.User
{
    public class GetUserResponse
    {
        public required Guid Id { get; set; }

        public required string Name { get; set; }

        public required string Email { get; set; }

        public required string UserName { get; set; }

        public required DateTime DateCreated { get; set; }

        public long Quota { get; set; }

        public long UsedQuota { get; set; }

        public required Role Role { get; set; }
    }
}
