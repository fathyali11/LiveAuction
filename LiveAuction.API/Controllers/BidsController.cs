using LiveAuction.Application.Bids.Commands.CreateBid;
using LiveAuction.Application.Bids.Queries.UserBidsHistory;
using LiveAuction.Domain.Consts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace LiveAuction.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class BidsController(IMediator mediator) : ControllerBase
{
    [EnableRateLimiting("place-bid")]
    [HttpPost("")]
    public async Task<IActionResult> Create([FromBody] CreateBidCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            error => error.Code switch
            {
                ErrorCodes.ValidationError => BadRequest(new { message = error.Message }),
                ErrorCodes.UnauthorizedError => Unauthorized(new { message = error.Message }),
                ErrorCodes.ForbiddenError => StatusCode(StatusCodes.Status403Forbidden, new { message = error.Message }),
                ErrorCodes.NotFoundError => NotFound(new { message = error.Message }),
                ErrorCodes.Conflict => Conflict(new { message = error.Message }),
                ErrorCodes.Duplicate => Conflict(new { message = error.Message }),
                _ => StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An unexpected error occurred." })
            },
            bidDto => Created($"/api/bids/{bidDto.Id}", bidDto));
    }

    [HttpGet("user-history")]
    public async Task<IActionResult> GetUserBidsHistory(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var query = new UserBidsHistoryQuery(userId);
        var result = await mediator.Send(query, cancellationToken);

        return result.Match<IActionResult>(
            error => error.Code switch
            {
                ErrorCodes.NotFoundError => NotFound(new { message = error.Message }),
                ErrorCodes.ValidationError => BadRequest(new { message = error.Message }),
                _ => StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An unexpected error occurred." })
            },
            userBids => Ok(userBids));
    }
}
