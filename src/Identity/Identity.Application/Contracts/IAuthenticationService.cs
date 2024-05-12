using Identity.Application.Features.Authentication;
using Identity.Domain.Enums;

namespace Identity.Application.Contracts;

public interface IAuthenticationService
{
    Task<RegistrationResult> Register(RegisterDTO registerModel);
}
