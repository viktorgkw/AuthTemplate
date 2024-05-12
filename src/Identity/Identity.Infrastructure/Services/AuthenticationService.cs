using Identity.Application.Contracts;
using Identity.Application.Features.Authentication;
using Identity.Domain.Enums;

namespace Identity.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService
{
    public async Task<RegistrationResult> Register(RegisterDTO registerModel)
    {
        await Task.Yield();

        return RegistrationResult.Successful;
    }
}
