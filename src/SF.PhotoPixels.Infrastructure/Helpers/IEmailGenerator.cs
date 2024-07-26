namespace SF.PhotoPixels.Infrastructure.Helpers;

public interface IEmailGenerator
{
    string CreatePasswordResetEmail(string code);

    string CreateAdminPasswordResetEmail(string name);

}