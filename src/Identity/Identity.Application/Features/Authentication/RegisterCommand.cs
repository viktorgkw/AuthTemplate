using Identity.Application.Contracts;
using Identity.Domain.Models;
using MediatR;

namespace Identity.Application.Features.Authentication;

public class RegisterCommand : IRequest<RegistrationResult>
{
    public RegisterDto Model { get; set; }
}

public class RegisterCommandHandler(
    IAuthenticationService authenticationService) : IRequestHandler<RegisterCommand, RegistrationResult>
{
    private readonly IAuthenticationService _authenticationService = authenticationService;

    public async Task<RegistrationResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
        => await _authenticationService.Register(request.Model);
}
