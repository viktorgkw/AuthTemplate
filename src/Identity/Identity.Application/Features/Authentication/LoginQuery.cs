using Identity.Application.Contracts;
using MediatR;

namespace Identity.Application.Features.Authentication;

public class LoginDto
{
    public string Email { get; set; }

    public string Password { get; set; }
}

public record LoginResult(string Token, string Message);

public class LoginQuery : IRequest<LoginResult>
{
    public LoginDto Model { get; set; }
}

public class LoginQueryHandler(IAuthenticationService authenticationService)
    : IRequestHandler<LoginQuery, LoginResult>
{
    private readonly IAuthenticationService _authenticationService = authenticationService;

    public async Task<LoginResult> Handle(LoginQuery request, CancellationToken cancellationToken)
        => await _authenticationService.Login(request.Model, cancellationToken);
}
