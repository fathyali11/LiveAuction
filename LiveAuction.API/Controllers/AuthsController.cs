using MediatR;
using Microsoft.AspNetCore.Mvc;
using LiveAuction.Application.ApplicationUsers.Commands.LoginUser;
using LiveAuction.Application.ApplicationUsers.Commands.RegisterUser;
using LiveAuction.Domain.Consts;

namespace LiveAuction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthsController(IMediator _mediator):ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);

        return result.Match<IActionResult>(
             error => error.Code switch
             {
                 ErrorCodes.NotFoundError => StatusCode(404, error.Message),
                 ErrorCodes.UnauthorizedError => StatusCode(401, error.Message),
                 ErrorCodes.ForbiddenError => StatusCode(403, error.Message),
                 ErrorCodes.ValidationError => BadRequest(error.Message),
                 _ => BadRequest(error.Message)
             },
             authResponse => Ok(authResponse));
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return result.Match<IActionResult>(
            error => error.Code switch
            {
                ErrorCodes.NotFoundError => StatusCode(404, error.Message),
                ErrorCodes.UnauthorizedError => StatusCode(401, error.Message),
                ErrorCodes.ForbiddenError => StatusCode(403, error.Message),
                ErrorCodes.ValidationError => BadRequest(error.Message),
                _ => BadRequest(error.Message)
            },
            authResponse => Ok(authResponse));
    }
}
