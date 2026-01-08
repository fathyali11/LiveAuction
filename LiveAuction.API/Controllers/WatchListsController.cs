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
    public async Task<IActionResult> GetWatchList([FromQuery] PaginatedRequest paginatedRequest, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var query = new GetWatchListQuery(userId, paginatedRequest);
        var result = await _mediator.Send(query,cancellationToken);

        return result.Match<IActionResult>(
            error => error.Code switch
            {
                ErrorCodes.NotFoundError => NotFound(error.Message),
                _ => StatusCode(500, "ÍÏË ÎØÃ ÛíÑ ãÊæÞÚ")
            },
            watchList => Ok(watchList));
    }
    [HttpPost("toggle/{id}")]
    public async Task<IActionResult> ToggleWatchListItem([FromRoute] int id,CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new ToggleWatchListItemCommand(userId, id), cancellationToken);
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