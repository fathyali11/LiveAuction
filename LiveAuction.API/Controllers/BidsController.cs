using LiveAuction.Application.Bids.Commands.CreateBid;
using LiveAuction.Application.Bids.Queries.UserBidsHistory;
using LiveAuction.Domain.Consts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LiveAuction.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class BidsController(IMediator mediator):ControllerBase
{
    [HttpPost("")]
    public async Task<IActionResult> Create([FromBody] CreateBidCommand command)
    {
        command.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await mediator.Send(command);
        return result.Match<IActionResult>(
            error => error.Code switch
            {
                ErrorCodes.NotFoundError => NotFound(error.Message),
                ErrorCodes.ValidationError => BadRequest(error.Message),
                _ => StatusCode(500, error.Message)
            },
            bidDto => CreatedAtAction(nameof(Create), new { id = bidDto.Id }, bidDto));
    }

    [HttpGet("user-history")]
    public async Task<IActionResult> GetUserBidsHistory()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var query = new UserBidsHistoryQuery(userId);
        var result = await mediator.Send(query);
        return result.Match<IActionResult>(
            error => error.Code switch
            {
                ErrorCodes.NotFoundError => NotFound(error.Message),
                ErrorCodes.ValidationError => BadRequest(error.Message),
                _ => StatusCode(500, "An unexpected error occurred.")
            },
            userBids => Ok(userBids));
    }
}
