using LiveAuction.Domain.Consts;
using LiveAuction.Shared.DTOs;
using MediatR;
using OneOf;

namespace LiveAuction.Application.Auctions.Queries.GetAuctionById;

public record GetAuctionByIdQuery(int Id) : IRequest<OneOf<Error, AuctionDto>>;