using LiveAuction.Domain.Consts;
using MediatR;
using OneOf;
using System;
using System.Collections.Generic;
using System.Text;

namespace LiveAuction.Application.Notifications.Commands.MarkAllAsRead;

public record MarkAllAsReadCommand(string UserId): IRequest<OneOf<Error, bool>>;
