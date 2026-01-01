using FluentValidation;
using LiveAuction.Application.Services.AuctionServices;
using LiveAuction.Domain.Consts;
using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Repositories;
using LiveAuction.Shared.DTOs;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OneOf;

namespace LiveAuction.Application.Auctions.Commands.CreateAuction;

internal class CreateAuctionCommandHandler(
    IAuctionRepository _auctionRepository,
    ILogger<CreateAuctionCommandHandler> _logger,
    IValidator<CreateAuctionCommand> _validator,
    UserManager<ApplicationUser> _userManager,
    IAuctionService _auctionService) : IRequestHandler<CreateAuctionCommand, OneOf<Error, AuctionDto>>
{
    public async Task<OneOf<Error, AuctionDto>> Handle(CreateAuctionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling CreateAuctionCommand for: {Title}", request.Title);

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var firstError = validationResult.Errors.First();
            _logger.LogWarning("Validation failed for CreateAuctionCommand: {Error}", firstError.ErrorMessage);
            return new Error(ErrorCodes.ValidationError, firstError.ErrorMessage);
        }

        var CreatedByUser = await _userManager.FindByIdAsync(request.CreatedById);
        if (CreatedByUser is null)
        {
            var errorMessage = $"User with ID {request.CreatedById} not found.";
            _logger.LogWarning(errorMessage);
            return new Error(ErrorCodes.NotFoundError, errorMessage);
        }
        if(!string.Equals(CreatedByUser.FullName,request.CreatedByName, StringComparison.Ordinal))
        {
            var errorMessage = $"User name mismatch for ID {request.CreatedById}.";
            _logger.LogWarning(errorMessage);
            return new Error(ErrorCodes.ValidationError, errorMessage);
        }


        var auction = request.Adapt<Auction>();
        auction.ImageName=await _auctionService.SaveImageAsync(request.Image, cancellationToken);

        await _auctionRepository.AddAsync(auction, cancellationToken);
        auction.JobId = await _auctionService.ScheduleAuction(auction, cancellationToken);
        await _auctionRepository.UpdateAsync(auction, cancellationToken);

        _logger.LogInformation("Auction created successfully: {Title} with ID: {Id}", auction.Title, auction.Id);

        var auctionDto = auction.Adapt<AuctionDto>();
        auctionDto.Bids = [];
        return auctionDto;
    }
}