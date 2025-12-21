using LiveAuction.Domain.Consts;
using LiveAuction.Domain.Repositories;
using LiveAuction.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using OneOf;

namespace LiveAuction.Application.Auctions.Commands.DeleteAuction;

internal class DeleteAuctionCommandHandler(
    IAuctionRepository _auctionRepository,
    ILogger<DeleteAuctionCommandHandler> _logger,
    IAuctionService _auctionService) : IRequestHandler<DeleteAuctionCommand, OneOf<Error, bool>>
{
    public async Task<OneOf<Error, bool>> Handle(DeleteAuctionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling DeleteAuctionCommand for ID: {Id}", request.Id);

        var auction = await _auctionRepository.GetByIdWithBidsAsync(request.Id, cancellationToken);
        
        if (auction == null)
        {
            _logger.LogWarning("Auction not found with ID: {Id}", request.Id);
            return new Error(ErrorCodes.NotFoundError, "«·„“«œ €Ì— „ÊÃÊœ");
        }

        if (auction.Bids.Any())
        {
            _logger.LogWarning("Cannot delete auction with ID: {Id} - Auction has {BidCount} existing bids", 
                request.Id, auction.Bids.Count);
            return new Error(ErrorCodes.ForbiddenError, "·« Ì„ﬂ‰ Õ–› „“«œ ÌÕ ÊÌ ⁄·Ï „“«Ìœ« . «·„“«œ ﬁœ Ã„⁄ √„Ê«·«.");
        }
        if(!string.Equals(auction.CreatedById, request.SellerId, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Unauthorized delete attempt on auction ID: {Id} by Seller ID: {SellerId}", 
                request.Id, request.SellerId);
            return new Error(ErrorCodes.UnauthorizedError, "€Ì— „”„ÊÕ ·ﬂ »Õ–› Â–« «·„“«œ ·√‰Â ·Ì” „·ﬂﬂ! ??û??");
        }
        await _auctionService.DeleteImageAsync(auction.ImageName, cancellationToken);
        await _auctionRepository.DeleteAsync(request.Id, cancellationToken);

        _logger.LogInformation("Auction deleted successfully: {Id}", request.Id);

        return true;
    }
}