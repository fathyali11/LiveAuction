using LiveAuction.Application.Services;
using LiveAuction.Domain.Consts;
using LiveAuction.Domain.Repositories;
using LiveAuction.Shared.DTOs;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using OneOf;

namespace LiveAuction.Application.Auctions.Queries.GetAllAuctions;

internal class GetAllAuctionsQueryHandler(
    IAuctionService _auctionService,
    ILogger<GetAllAuctionsQueryHandler> _logger) : IRequestHandler<GetAllAuctionsQuery, OneOf<Error, PaginatedResult<AuctionsInHomePageDto>>>
{
    public async Task<OneOf<Error, PaginatedResult<AuctionsInHomePageDto>>> Handle(GetAllAuctionsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetAllAuctionsQuery");
        var auctions = await _auctionService.GetAllActiveAuctionsAsync(request.UserId, request.PaginatedRequest, cancellationToken);
        _logger.LogInformation("Successfully retrieved auctions for GetAllAuctionsQuery");
        return auctions;
    }
}