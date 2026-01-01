using LiveAuction.Domain.Consts;
using MediatR;

namespace LiveAuction.Application.Wallets.Commands.Deposit;
public record DepositCommand(string UserId, decimal Amount): IRequest<Error>;
