using SF.PhotoPixels.Infrastructure.Helpers;

namespace SF.PhotoPixels.API.Integration.Tests.FakeServices;

internal class FakeEmailGenerator : IEmailGenerator
{
    private TotpState _state;

    public FakeEmailGenerator(TotpState state)
    {
        _state = state;
    }

    public string CreatePasswordResetEmail(string code)
    {
        _state.Totp = code;

        return string.Empty;
    }

    public string CreateAdminPasswordResetEmail(string name)
    {
        return string.Empty;
    }
}