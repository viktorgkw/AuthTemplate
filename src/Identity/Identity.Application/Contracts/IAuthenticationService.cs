using Identity.Application.Features.Authentication;

namespace Identity.Application.Contracts;

public interface IAuthenticationService
{
    Task<LoginResult> Login(LoginDto loginModel, CancellationToken cancellationToken);

    Task<RegistrationResult> Register(RegisterDto registerModel, CancellationToken cancellationToken);
}
