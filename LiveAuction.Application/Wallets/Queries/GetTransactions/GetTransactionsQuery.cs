using LiveAuction.Domain.Consts;
using LiveAuction.Shared.DTOs;
using MediatR;
using OneOf;

namespace LiveAuction.Application.Wallets.Queries.GetTransactions;
public record GetTransactionsQuery(string UserId, int PageNumber, int PageSize): IRequest<OneOf<Error, PaginatedResult<TransactionResponse>>>;
