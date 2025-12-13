using LiveAuction.Domain.Consts;
using LiveAuction.Domain.Repositories;
using LiveAuction.Shared.DTOs;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using OneOf;

namespace LiveAuction.Application.Auctions.Queries.GetAuctionById;

internal class GetAuctionByIdQueryHandler(
    IAuctionRepository _auctionRepository,
    ILogger<GetAuctionByIdQueryHandler> _logger) : IRequestHandler<GetAuctionByIdQuery, OneOf<Error, AuctionDto>>
{
    public async Task<OneOf<Error, AuctionDto>> Handle(GetAuctionByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetAuctionByIdQuery for ID: {Id}", request.Id);

        var auction = await _auctionRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (auction == null)
        {
            _logger.LogWarning("Auction not found with ID: {Id}", request.Id);
            return new Error(ErrorCodes.NotFoundError, "«·„“«œ €Ì— „ÊÃÊœ");
        }

        _logger.LogInformation("Auction found with ID: {Id}", request.Id);
        
        return auction.Adapt<AuctionDto>();
    }
}