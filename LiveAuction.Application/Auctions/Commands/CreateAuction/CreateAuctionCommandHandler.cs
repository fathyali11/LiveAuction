using FluentValidation;
using LiveAuction.Application.Interfaces;
using LiveAuction.Application.Services.AuctionServices;
using LiveAuction.Domain.Consts;
using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Repositories;
using LiveAuction.Shared.DTOs;
using LiveAuction.Shared.Enums;
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
    IFileService _fileService,
    IUserService _userService,
    IAuctionService _auctionService) : IRequestHandler<CreateAuctionCommand, OneOf<Error, AuctionDto>>
{
    public async Task<OneOf<Error, AuctionDto>> Handle(CreateAuctionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Handling CreateAuctionCommand for: {Title}", request.Title);

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var firstError = validationResult.Errors.First();
            _logger.LogWarning("Validation failed for CreateAuctionCommand: {Error}", firstError.ErrorMessage);
            return new Error(ErrorCodes.ValidationError, firstError.ErrorMessage);
        }

        var currentUserId = _userService.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            _logger.LogWarning("Cannot create an auction without an authenticated user.");
            return new Error(ErrorCodes.UnauthorizedError, "You must be signed in to create an auction.");
        }

        var createdByUser = await _userManager.FindByIdAsync(currentUserId);
        if (createdByUser is null)
        {
            _logger.LogWarning("User {UserId} not found.", currentUserId);
            return new Error(ErrorCodes.NotFoundError, "User not found.");
        }

        string? imageName = null;
        string? jobId = null;

        try
        {
            imageName = await _fileService.SaveFileAsync(request.Image, cancellationToken);
            if (imageName is null)
            {
                _logger.LogError("Failed to save the auction image for user {UserId}.", currentUserId);
                return new Error(ErrorCodes.InternalServerError, "Failed to save the auction image.");
            }

            var auction = request.Adapt<Auction>();
            auction.ImageName = imageName;
            auction.CreatedById = currentUserId;
            auction.CreatedBy = createdByUser;
            auction.Status = AuctionStatus.Open;

            await using var transaction = await _auctionRepository.BeginTransactionAsync(cancellationToken);

            await _auctionRepository.AddAsync(auction, cancellationToken);

            jobId = await _auctionService.ScheduleAuction(auction, cancellationToken);
            auction.JobId = jobId;

            await _auctionRepository.SaveChangesAsync(cancellationToken);

            var auctionDto = auction.Adapt<AuctionDto>();
            auctionDto.Bids = [];

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Auction created successfully: {Title} with ID: {Id}",
                auction.Title, auction.Id);

            return auctionDto;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Auction creation was cancelled for user {UserId}.", currentUserId);
            await CompensateAsync(imageName, jobId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating the auction for user {UserId}.", currentUserId);
            await CompensateAsync(imageName, jobId);
            return new Error(ErrorCodes.InternalServerError, "An error occurred while creating the auction.");
        }
    }
    private async Task CompensateAsync(string? imageName, string? jobId)
    {
        if (jobId is not null)
        {
            try
            {
                await _auctionService.CancelScheduledAuction(jobId, CancellationToken.None);
                _logger.LogInformation("Rolled back scheduled job: {JobId}", jobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel scheduled job {JobId} during compensation.", jobId);
            }
        }

        if (imageName is not null)
        {
            try
            {
                await _fileService.DeleteFileAsync(imageName, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete image {ImageName} during compensation.", imageName);
            }
        }
    }
}
