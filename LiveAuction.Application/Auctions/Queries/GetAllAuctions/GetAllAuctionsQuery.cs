using LiveAuction.Shared.DTOs;
using MediatR;

namespace LiveAuction.Application.Auctions.Queries.GetAllAuctions;

public record GetAllAuctionsQuery : IRequest<List<AuctionDto>>;