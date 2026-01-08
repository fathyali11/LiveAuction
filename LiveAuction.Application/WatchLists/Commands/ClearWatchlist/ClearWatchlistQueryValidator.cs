using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace LiveAuction.Application.WatchLists.Commands.ClearWatchlist;

internal class ClearWatchlistQueryValidator:AbstractValidator<ClearWatchlistQuery>
{
    public ClearWatchlistQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId cannot be empty.");
    }
}
