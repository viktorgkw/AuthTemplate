using Identity.Application.Contracts;
using MediatR;

namespace Identity.Application.Features.Authentication;

public class RegisterDto
{
    public string Email { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string PhoneNumber { get; set; }
}

public enum RegistrationStatus
{
    Failed = 0,
    EmailInUse = 2,
    Successful = 4
}

public record RegistrationResult(RegistrationStatus Status, string Message);

public class RegisterCommand : IRequest<RegistrationResult>
{
    public RegisterDto Model { get; set; }
}

public class RegisterCommandHandler(IAuthenticationService authenticationService)
    : IRequestHandler<RegisterCommand, RegistrationResult>
{
    private readonly IAuthenticationService _authenticationService = authenticationService;

    public async Task<RegistrationResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
        => await _authenticationService.Register(request.Model, cancellationToken);
}
