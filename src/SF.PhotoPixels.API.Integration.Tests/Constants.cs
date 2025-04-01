namespace SF.PhotoPixels.API.Integration.Tests;

internal class Constants
{
    internal readonly static (string Email, string Password) SeededAdminCredentials = new("admin@admin.com", "P@ssword1");
    internal readonly static (string Email, string Password) DefaultContributorCredentials = new("testUser@test.com", "P@ssword1");
    internal static readonly string WhiteimagePath = "Assets/whiteimage.jpg";
    internal static readonly string WhiteimageHash = "abcXifOlGhiHcVyfDvDGOsGANE4=";
    internal static readonly string RunVideoPath = "Assets/run.mp4";
    internal static readonly string RunVideoHash = "xOxBAwLexkGgSxuIj5Vqy9z5oM4=";
}
