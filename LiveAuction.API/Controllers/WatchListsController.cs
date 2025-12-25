using LiveAuction.Application.WatchLists.Commands.ToggleWatchListItem;
using LiveAuction.Application.WatchLists.Queries.GetWatchList;
using LiveAuction.Application.WatchLists.Queries.GetWatchListCount;
using LiveAuction.Domain.Consts;
using LiveAuction.Shared.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LiveAuction.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class WatchListsController(IMediator _mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetWatchList()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var query = new GetWatchListQuery(userId);
        var result = await _mediator.Send(query);

        return result.Match<IActionResult>(
            error => error.Code switch
            {
                ErrorCodes.NotFoundError => NotFound(error.Message),
                _ => StatusCode(500, "ÍÏË ÎØÃ ÛíÑ ãÊæÞÚ")
            },
            watchList => Ok(watchList));
    }
    [HttpPost("toggle")]
    public async Task<IActionResult> ToggleWatchListItem(ToggleWatchListItemRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var command = new ToggleWatchListItemCommand(userId, request.AuctionId, request.Title, request.ImageName, request.CurrentPrice, request.EndTime);
        var result = await _mediator.Send(command);
        return result.Match<IActionResult>(
            error => error.Code switch
            {
                ErrorCodes.ValidationError => BadRequest(error.Message),
                ErrorCodes.NotFoundError => NotFound(error.Message),
                _ => StatusCode(500, "ÍÏË ÎØÃ ÛíÑ ãÊæÞÚ")
            },
            response => Ok(response));
    }

    [AllowAnonymous]
    [HttpGet("count")]
    public async Task<IActionResult> GetWatchListCount()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var query = new GetWatchListCountQuery(userId);
        var count = await _mediator.Send(query);
        return Ok(count);
    }
}