namespace SF.PhotoPixels.Domain.Models
{
    public class ConvertVideoCommand
    {
        public required string Filename { get; set; }
        public required string objectPath { get; set; }
        public required string convertedVideoPath { get; set; }
        public required string thumbVideoPath { get; set; }
        public required Guid UserId { get; set; }
    }
}
