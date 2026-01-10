using LiveAuction.Application.Interfaces;
using LiveAuction.Application.Services.BackgroundJobServices;
using LiveAuction.Application.Services.NotificationServices;
using LiveAuction.Application.Services.WalletServices;
using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Repositories;
using LiveAuction.Shared.DTOs;
using LiveAuction.Shared.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LiveAuction.Application.Services.AuctionServices;

internal class AuctionService(IBackgroundJobService _backgroundJobService,
    IAuctionNotificationService _auctionNotificationService,
    IAuctionRepository _auctionRepository,
    IServiceProvider _serviceProvider) : IAuctionService
{
    public async Task<PaginatedResult<AuctionsInHomePageDto>> GetAllActiveAuctionsAsync(string? userId, PaginatedRequest paginatedRequest, CancellationToken cancellationToken)
    {
        paginatedRequest.PageNumber = paginatedRequest.PageNumber <= 0 ? 1 : paginatedRequest.PageNumber;
        paginatedRequest.PageNumber = paginatedRequest.PageNumber <= 0 ? 8 : paginatedRequest.PageNumber;
        var (auctions , count) = await _auctionRepository.GetAllActiveAndItsCountAsync(userId,paginatedRequest, cancellationToken);
        return new PaginatedResult<AuctionsInHomePageDto>
        {
            Items = auctions,
            TotalCount = count,
            PageNumber = paginatedRequest.PageNumber,
            PageSize = paginatedRequest.PageSize
        };
    }
    public async Task<string> SaveImageAsync(IFormFile image, CancellationToken cancellationToken)
    {
        var imageName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
        var imageFolder = Path.Combine("wwwroot", "images");
        if (!Directory.Exists(imageFolder))
        {
            Directory.CreateDirectory(imageFolder);
        }
        var imagePath = Path.Combine(imageFolder, imageName);
        using var stream = new FileStream(imagePath, FileMode.Create);
        await image.CopyToAsync(stream, cancellationToken);

        return imageName;
    }
    public async Task DeleteImageAsync(string imageName, CancellationToken cancellationToken)
    {
        var imagePath = Path.Combine("wwwroot", "images", imageName);
        if (File.Exists(imagePath))
        {
            await Task.Run(() => File.Delete(imagePath), cancellationToken);
        }
    }

    public async Task<string> ScheduleAuction(Auction auction,CancellationToken cancellationToken)
    {
        var jobId = _backgroundJobService.ScheduleJob<IAuctionService>(
            repo => repo.TerminateAuctionAsync(auction.Id, cancellationToken),
            auction.EndTime - DateTime.UtcNow
            );
        return jobId;
    }
    
    public async Task TerminateAuctionAsync(int auctionId, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var auctionRepository = scope.ServiceProvider.GetRequiredService<IAuctionRepository>();
        var walletService = scope.ServiceProvider.GetRequiredService<IWalletService>();
        var transaction = await auctionRepository.BeginTransactionAsync(cancellationToken);
        try
        {
            var auction = await auctionRepository.GetAuctionToTerminateAsync(auctionId, cancellationToken);
            if (auction is null || auction.Status != AuctionStatus.Open)
                return;
            auction.Status = AuctionStatus.Closed;
            string? winnerIdToNotify = null;
            string? sellerIdToNotify = null;
            if (auction.Bids.Any())
            {
                var highestBid = auction.Bids.OrderByDescending(b => b.Amount).First();
                auction.WinnerId = highestBid.BidderId;
                auction.CurrentBidderId = highestBid.BidderId;

                var isMoneyTransfered= await walletService
                    .TransferMoneyAsync(highestBid.BidderId, auction.CreatedById, highestBid.Amount,auctionId, cancellationToken);
                if (!isMoneyTransfered)
                {
                    throw new Exception("Money transfer failed");
                }
                winnerIdToNotify = highestBid.BidderId;
                sellerIdToNotify = auction.CreatedById;
                
            }

            await auctionRepository.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            if (winnerIdToNotify is not null)
            {
                Console.WriteLine($"Notifying winner {winnerIdToNotify} and seller {sellerIdToNotify}");
                await _auctionNotificationService
                    .ForceRefreshWalletAsync(winnerIdToNotify);
                await _auctionNotificationService
                    .ForceRefreshWalletAsync(sellerIdToNotify!);
                var notificationWinner = new NotificationDto
                (
                    UserId: winnerIdToNotify!,
                    Title: "Auction Won!",
                    Message: $"Congratulations! You have won the auction for '{auction.Title}' with a bid of {auction.Bids.Max(b => b.Amount):C}.",
                    false,
                    NotificationType: NotificationType.Auction,
                    RelatedEntityId: auction.Id
                );
                var notificationSeller = new NotificationDto
                    (
                        UserId: sellerIdToNotify!,
                        Title: "Auction Ended!",
                        Message: $"Your auction for '{auction.Title}' has ended. The winning bid was {auction.Bids.Max(b => b.Amount):C}.",
                        false,
                        NotificationType: NotificationType.Auction,
                        RelatedEntityId: auction.Id
                    );
                _backgroundJobService.EnqueueJob<INotificationService>(
                x => x.AddNotificationAsync(notificationWinner, cancellationToken)
                );
                await _auctionNotificationService.AddNotification(winnerIdToNotify, notificationWinner);

                _backgroundJobService.EnqueueJob<INotificationService>(
                x => x.AddNotificationAsync(notificationSeller, cancellationToken)
                );
                await _auctionNotificationService.AddNotification(sellerIdToNotify!, notificationSeller);
            }
            


        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }


}
