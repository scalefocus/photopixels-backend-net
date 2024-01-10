using Mediator;
using Microsoft.AspNetCore.Identity;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application.Security.BearerToken;

namespace SF.PhotoPixels.Application.Query.User.Login;

public class LoginHandler : IRequestHandler<LoginRequest, OneOf<Success, Unauthorized>>
{
    private readonly SignInManager<Domain.Entities.User> _signInManager;
    private readonly UserManager<Domain.Entities.User> _userManager;

    public LoginHandler(SignInManager<Domain.Entities.User> signInManager, UserManager<Domain.Entities.User> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public async ValueTask<OneOf<Success, Unauthorized>> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        _signInManager.AuthenticationScheme = BearerTokenDefaults.AuthenticationScheme;

        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            return new Unauthorized();
        }

        var result = await _signInManager.PasswordSignInAsync(user, request.Password, false, false);

        if (!result.Succeeded)
        {
            return new Unauthorized();
        }

        return new Success();
    }
}