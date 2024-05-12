using Identity.Domain.Enums;
using Identity.Domain.Models;

namespace Identity.Application.Contracts;

public interface IAuthenticationService
{
    Task<RegistrationResult> Register(RegisterDto registerModel);
}
