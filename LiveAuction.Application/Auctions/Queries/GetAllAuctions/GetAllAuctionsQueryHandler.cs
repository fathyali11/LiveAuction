using LiveAuction.Domain.Repositories;
using LiveAuction.Shared.DTOs;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LiveAuction.Application.Auctions.Queries.GetAllAuctions;

internal class GetAllAuctionsQueryHandler(
    IAuctionRepository _auctionRepository,
    ILogger<GetAllAuctionsQueryHandler> _logger) : IRequestHandler<GetAllAuctionsQuery, List<AuctionDto>>
{
    public async Task<List<AuctionDto>> Handle(GetAllAuctionsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetAllAuctionsQuery");

        var auctions = await _auctionRepository.GetAllActiveAsync(cancellationToken);

        return auctions.Adapt<List<AuctionDto>>();
    }
}