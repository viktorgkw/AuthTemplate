using Identity.Application.Features.Authentication;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

[ApiController, Route("[controller]")]
public class IdentityController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpPost, Route("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterDto model)
    {
        var result = await _mediator.Send(new RegisterCommand
        {
            Model = model
        });

        return result.Status switch
        {
            RegistrationStatus.Successful => Ok(result.Message),
            _ => BadRequest(result.Message)
        };
    }

    [HttpPost, Route("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(LoginDto model)
    {
        var result = await _mediator.Send(new LoginQuery
        {
            Model = model
        });

        return result.Token is null
            ? BadRequest(result.Message)
            : Ok(result.Token);
    }
}
