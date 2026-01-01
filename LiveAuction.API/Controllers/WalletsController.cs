using LiveAuction.Application.Wallets.Commands.Deposit;
using LiveAuction.Domain.Consts;
using LiveAuction.Shared.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LiveAuction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WalletsController(IMediator _mediator): ControllerBase
{
    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] DepositRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var command = new DepositCommand(userId!, request.Amount);
        var result = await _mediator.Send(command, cancellationToken);
        return result.Code switch
        {
            ErrorCodes.None => Ok(),
            ErrorCodes.ValidationError => BadRequest(result),
            _ => StatusCode(500, result)
        };
    }
}
