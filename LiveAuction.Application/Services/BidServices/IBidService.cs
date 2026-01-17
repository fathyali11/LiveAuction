using LiveAuction.Application.Bids.Commands.CreateBid;
using LiveAuction.Domain.Consts;
using LiveAuction.Shared.DTOs;
using OneOf;
using System;
using System.Collections.Generic;
using System.Text;

namespace LiveAuction.Application.Services.BidServices;

internal interface IBidService
{
    Task<OneOf<Error, BidDto>> AddBidAsync(CreateBidCommand createBidCommand, CancellationToken cancellationToken = default);
}
