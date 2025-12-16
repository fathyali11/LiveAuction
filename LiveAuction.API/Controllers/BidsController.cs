using LiveAuction.Application.Bids.Commands.CreateBid;
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
                ErrorCodes.NotFoundError => NotFound(),
                ErrorCodes.ValidationError => BadRequest(error.Message),
                _ => StatusCode(500, "An unexpected error occurred.")
            },
            bidDto => CreatedAtAction(nameof(Create), new { id = bidDto.Id }, bidDto));
    }
}
