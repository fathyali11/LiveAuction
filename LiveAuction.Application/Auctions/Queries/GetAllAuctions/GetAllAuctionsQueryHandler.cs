using LiveAuction.Domain.Repositories;
using LiveAuction.Shared.DTOs;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LiveAuction.Application.Auctions.Queries.GetAllAuctions;

internal class GetAllAuctionsQueryHandler(
    IAuctionRepository _auctionRepository,
    ILogger<GetAllAuctionsQueryHandler> _logger) : IRequestHandler<GetAllAuctionsQuery, List<AuctionsInHomePageDto>>
{
    public async Task<List<AuctionsInHomePageDto>> Handle(GetAllAuctionsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetAllAuctionsQuery");

        var auctions = await _auctionRepository.GetAllActiveAsync(request.UserId,cancellationToken);

        return auctions;
    }
}