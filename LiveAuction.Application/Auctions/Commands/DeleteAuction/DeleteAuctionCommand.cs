using LiveAuction.Domain.Consts;
using MediatR;
using OneOf;

namespace LiveAuction.Application.Auctions.Commands.DeleteAuction;

public record DeleteAuctionCommand(int Id,string SellerId) : IRequest<OneOf<Error, bool>>;