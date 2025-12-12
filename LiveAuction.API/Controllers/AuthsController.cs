using MediatR;
using Microsoft.AspNetCore.Mvc;
using LiveAuction.Application.ApplicationUsers.Commands.LoginUser;
using LiveAuction.Application.ApplicationUsers.Commands.RegisterUser;
using LiveAuction.Domain.Consts;
using LiveAuction.Shared.DTOs;

namespace LiveAuction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthsController(IMediator _mediator):ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RegisterUserCommand(request.FullName,request.Email,request.Password), cancellationToken);

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
    public async Task<IActionResult> Login([FromBody] LoginRequest  request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new LoginUserCommand(request.Email, request.Password), cancellationToken);
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
