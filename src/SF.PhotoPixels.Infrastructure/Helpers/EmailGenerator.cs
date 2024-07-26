namespace SF.PhotoPixels.Infrastructure.Helpers;

public class EmailGenerator : IEmailGenerator
{
    public string CreatePasswordResetEmail(string code)
    {
        var mailBody = GetType().GetEmbeddedFileContent("ForgotPasswordTemplate.html");

        return mailBody.Replace("[code]", code);
    }

    public string CreateAdminPasswordResetEmail(string name)
    {
        var mailBody = GetType().GetEmbeddedFileContent("AdminResetPasswordTemplate.html");

        return mailBody.Replace("[name]", name);
    }
}