using Identity.Application.Features.Authentication;
using SharedKernel.Models.Interfaces;

namespace Identity.Application.Contracts;

public interface IAuthenticationService : IService
{
    Task<LoginResult> Login(LoginDto loginModel, CancellationToken cancellationToken);

    Task<RegistrationResult> Register(RegisterDto registerModel, CancellationToken cancellationToken);
}
