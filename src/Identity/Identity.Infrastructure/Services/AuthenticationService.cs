using Identity.Application.Contracts;
using Identity.Domain.Enums;
using Identity.Domain.Models;

namespace Identity.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService
{
    public async Task<RegistrationResult> Register(RegisterDto registerModel)
    {
        // IUserRepository (add to db)
        return new RegistrationResult(RegistrationStatus.Successful, "TODO");
    }
}
