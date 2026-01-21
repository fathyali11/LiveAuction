using LiveAuction.Application.Auctions.Commands.CreateAuction;
using LiveAuction.Application.Auctions.Commands.DeleteAuction;
using LiveAuction.Application.Auctions.Queries.GetAllAuctions;
using LiveAuction.Application.Auctions.Queries.GetAuctionById;
using LiveAuction.Domain.Consts;
using LiveAuction.Shared.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LiveAuction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuctionsController(IMediator _mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAll([FromQuery] PaginatedRequest paginatedRequest, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _mediator.Send(new GetAllAuctionsQuery(userId,paginatedRequest), cancellationToken);
        return result.Match<ActionResult<List<AuctionDto>>>(
            error => BadRequest(new { message = error.Message }),
            paginatedResult => Ok(paginatedResult));
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute]int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAuctionByIdQuery(id), cancellationToken);

        return result.Match<IActionResult>(
            error => error.Code switch
            {
                ErrorCodes.NotFoundError => NotFound(new { message = error.Message }),
                _ => BadRequest(new { message = error.Message })
            },
            auctionDto => Ok(auctionDto));
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateAuctionRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateAuctionCommand(
            request.Title,
            request.Description,
            request.Image,
            request.StartTime,
            request.StartingBid,
            request.DurationInMinutes,
            User.FindFirstValue(ClaimTypes.NameIdentifier)!,
            request.Seller);

        var result = await _mediator.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            error => error.Code switch
            {
                ErrorCodes.ValidationError => BadRequest(error.Message),
                _ => BadRequest(error.Message)
            },
            auctionDto => CreatedAtAction(nameof(GetById), new { id = auctionDto.Id }, auctionDto));
    }


    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteAuctionCommand(id,User.FindFirstValue(ClaimTypes.NameIdentifier)!)
            , cancellationToken);

        return result.Match<IActionResult>(
            error => error.Code switch
            {
                ErrorCodes.NotFoundError => NotFound(error.Message),
                _ => BadRequest(error.Message)
            },
            success => NoContent());
    }
}