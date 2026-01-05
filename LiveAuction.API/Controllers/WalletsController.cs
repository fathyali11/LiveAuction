using LiveAuction.Application.Wallets.Commands.Deposit;
using LiveAuction.Application.Wallets.Queries.GetTransactions;
using LiveAuction.Application.Wallets.Queries.GetWalletSummary;
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
    [HttpGet("summary")]
    public async Task<IActionResult> GetWalletSummary(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var query = new GetWalletSummaryQuery(userId!);
        var result = await _mediator.Send(query, cancellationToken);
        return result.Match<IActionResult>(
            error => error.Code switch
            {
                ErrorCodes.ValidationError => BadRequest(error),
                ErrorCodes.NotFoundError => NotFound(error),
                _ => StatusCode(500, error)
            },
            wallet => Ok(wallet));
    }
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var query = new GetTransactionsQuery(userId!, pageNumber, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return result.Match<IActionResult>(
            error => error.Code switch
            {
                ErrorCodes.ValidationError => BadRequest(error),
                _ => StatusCode(500, error)
            },
            transaction => Ok(transaction));
    }
}
