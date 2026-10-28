using LiveAuction.Application.Interfaces;
using LiveAuction.Application.Services;
using LiveAuction.Domain.Consts;
using LiveAuction.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using OneOf;

namespace LiveAuction.Application.Auctions.Commands.DeleteAuction;

internal class DeleteAuctionCommandHandler(
    IAuctionRepository _auctionRepository,
    ILogger<DeleteAuctionCommandHandler> _logger,
    IFileService _fileService,
    IAuctionService _auctionService) : IRequestHandler<DeleteAuctionCommand, OneOf<Error, bool>>
{
    public async Task<OneOf<Error, bool>> Handle(DeleteAuctionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Handling DeleteAuctionCommand for ID: {Id}", request.Id);

        var auction = await _auctionRepository.GetByIdWithBidsAsync(request.Id, cancellationToken);

        if (auction is null)
        {
            _logger.LogWarning("Auction not found with ID: {Id}", request.Id);
            return new Error(ErrorCodes.NotFoundError, "المزاد غير موجود.");
        }

        if (auction.Bids.Count != 0)
        {
            _logger.LogWarning("Cannot delete auction with ID: {Id} - Auction has {BidCount} existing bids",
                request.Id, auction.Bids.Count);
            return new Error(ErrorCodes.ForbiddenError, "لا يمكن حذف المزاد لوجود مزايدات عليه.");
        }

        if (!string.Equals(auction.CreatedById, request.SellerId, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Unauthorized delete attempt on auction ID: {Id} by Seller ID: {SellerId}",
                request.Id, request.SellerId);
            return new Error(ErrorCodes.ForbiddenError, "ليس لديك صلاحية لحذف هذا المزاد.");
        }

        await _auctionRepository.DeleteAsync(request.Id, cancellationToken);

        try
        {
            await _fileService.DeleteFileAsync(auction.ImageName, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete image {ImageName} for auction ID: {Id}",
                auction.ImageName, request.Id);
        }

        if (!string.IsNullOrWhiteSpace(auction.JobId))
        {
            try
            {
                await _auctionService.CancelScheduledAuction(auction.JobId, CancellationToken.None);
                _logger.LogInformation("Scheduled job deleted successfully for auction ID: {Id}, Job ID: {JobId}",
                    request.Id, auction.JobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete scheduled job {JobId} for auction ID: {Id}",
                    auction.JobId, request.Id);
            }
        }

        _logger.LogInformation("Auction deleted successfully: {Id}", request.Id);

        return true;
    }
}
