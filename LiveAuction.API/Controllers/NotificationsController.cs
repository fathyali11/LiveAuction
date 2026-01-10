using LiveAuction.Application.Notifications.Commands.MarkAllAsRead;
using LiveAuction.Application.Notifications.Queries.GetAllNotifications;
using LiveAuction.Application.Notifications.Queries.GetCountUnRead;
using LiveAuction.Domain.Consts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LiveAuction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController(IMediator _mediator):ControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> GetAllNotifications(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var query = new GetAllNotificationsQuery(userId);
        var result = await _mediator.Send(query, cancellationToken);
        return result.Match<IActionResult>(
            error => error.Code switch
            {
                ErrorCodes.ValidationError => BadRequest(error),
                _ => StatusCode(StatusCodes.Status500InternalServerError, error)
            },
            notifications => Ok(notifications));
    }
    [HttpGet("unread/count")]
    public async Task<IActionResult> GetCountUnRead(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var query = new GetCountUnReadQuery(userId);
        var result = await _mediator.Send(query, cancellationToken);
        return result.Match<IActionResult>(
            error => error.Code switch
            {
                ErrorCodes.ValidationError => BadRequest(error),
                _ => StatusCode(StatusCodes.Status500InternalServerError, error)
            },
            count => Ok(count));
    }
    [HttpPut("mark-all-as-read")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var command = new MarkAllAsReadCommand(userId);
        var result = await _mediator.Send(command, cancellationToken);
        return result.Match<IActionResult>(
            error => error.Code switch
            {
                ErrorCodes.ValidationError => BadRequest(error),
                ErrorCodes.NotFoundError => NotFound(error),
                _ => StatusCode(StatusCodes.Status500InternalServerError, error)
            },
            isUpdated => Ok(isUpdated));
    }
    [HttpPut("mark-as-read/{notificationId}")]
    public async Task<IActionResult> MarkAsRead([FromRoute] int notificationId,CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var command = new MarkAllAsReadCommand(userId);
        var result = await _mediator.Send(command, cancellationToken);
        return result.Match<IActionResult>(
            error => error.Code switch
            {
                ErrorCodes.ValidationError => BadRequest(error),
                ErrorCodes.NotFoundError => NotFound(error),
                _ => StatusCode(StatusCodes.Status500InternalServerError, error)
            },
            isUpdated => Ok(isUpdated));
    }

}
