namespace SF.PhotoPixels.API.Extensions
{
    public static class FormFileExtensions
    {
        public static bool SupportFormats(this IFormFile file, string[] allowedExtensions)
        {
            var extension = Path.GetExtension(file.FileName.ToLower());
            return allowedExtensions.Contains(extension);
        }
    }
}
