using LiveAuction.Domain.Consts;
using LiveAuction.Shared.DTOs;
using MediatR;
using OneOf;

namespace LiveAuction.Application.Auctions.Queries.GetAllAuctions;

public record GetAllAuctionsQuery(string? UserId,
    int PageNumber, int PageSize
    ) : IRequest<OneOf<Error, PaginatedResult<AuctionsInHomePageDto>>>;
