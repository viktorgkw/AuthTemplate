using Identity.Application.Features.Authentication;
using Identity.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class IdentityController(
    ILogger<IdentityController> logger,
    IMediator mediator) : ControllerBase
{
    private readonly ILogger<IdentityController> _logger = logger;
    private readonly IMediator _mediator = mediator;

    [HttpPost, Route("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterDTO model)
    {
        var result = await _mediator.Send(new RegisterCommand
        {
            Model = model
        });

        return result switch
        {
            RegistrationResult.Successful => NoContent(),
            _ => BadRequest()
        };
    }

    [HttpPost, Route("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login()
    {
        await Task.Yield();
        return Ok();
    }
}
