using LiveAuction.Domain.Consts;
using LiveAuction.Shared.DTOs;
using MediatR;
using OneOf;

namespace LiveAuction.Application.Wallets.Queries.GetTransactions;
public record GetTransactionsQuery(string UserId, PaginatedRequest PaginatedRequest) : IRequest<OneOf<Error, PaginatedResult<TransactionResponse>>>;
