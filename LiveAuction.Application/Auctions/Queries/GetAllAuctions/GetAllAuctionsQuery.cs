using LiveAuction.Domain.Consts;
using LiveAuction.Shared.DTOs;
using MediatR;
using OneOf;

namespace LiveAuction.Application.Auctions.Queries.GetAllAuctions;

public record GetAllAuctionsQuery(string? UserId,
    PaginatedRequest PaginatedRequest
    ) : IRequest<OneOf<Error, PaginatedResult<AuctionsInHomePageDto>>>;
