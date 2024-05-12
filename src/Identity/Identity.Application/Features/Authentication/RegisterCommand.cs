using Identity.Application.Contracts;
using Identity.Domain.Enums;
using MediatR;

namespace Identity.Application.Features.Authentication;

public class RegisterDTO
{
    public string Email { get; set; }

    public string Username { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string PhoneNumber { get; set; }
}

public class RegisterCommand : IRequest<RegistrationResult>
{
    public RegisterDTO Model { get; set; }
}

public class RegisterCommandHandler(
    IAuthenticationService authenticationService) : IRequestHandler<RegisterCommand, RegistrationResult>
{
    private readonly IAuthenticationService _authenticationService = authenticationService;

    public async Task<RegistrationResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
        => await _authenticationService.Register(request.Model);
}
