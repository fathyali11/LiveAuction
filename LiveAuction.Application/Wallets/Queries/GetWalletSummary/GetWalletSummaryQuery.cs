using LiveAuction.Domain.Consts;
using LiveAuction.Shared.DTOs;
using MediatR;
using OneOf;

namespace LiveAuction.Application.Wallets.Queries.GetWalletSummary;
public record GetWalletSummaryQuery(string UserId) : IRequest<OneOf<Error,WalletSummaryResponse>>;
