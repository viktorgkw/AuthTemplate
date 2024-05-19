using Identity.Application.Features.Authentication;

namespace Identity.Application.Contracts;

public interface IAuthenticationService
{
    Task<RegistrationResult> Register(RegisterDto registerModel);
}
