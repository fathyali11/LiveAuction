using LiveAuction.Application.Interfaces;
using LiveAuction.Application.Services.BackgroundJobServices;
using LiveAuction.Application.Services.NotificationServices;
using LiveAuction.Application.Services.WalletServices;
using LiveAuction.Domain.Entities;
using LiveAuction.Domain.Repositories;
using LiveAuction.Shared.DTOs;
using LiveAuction.Shared.Enums;
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
        paginatedRequest.PageSize = paginatedRequest.PageSize <= 0 ? 8 : paginatedRequest.PageSize;
        var (auctions , count) = await _auctionRepository.GetAllActiveAndItsCountAsync(userId,paginatedRequest, cancellationToken);
        return new PaginatedResult<AuctionsInHomePageDto>
        {
            Items = auctions,
            TotalCount = count,
            PageNumber = paginatedRequest.PageNumber,
            PageSize = paginatedRequest.PageSize
        };
    }

    public Task<string> ScheduleAuction(Auction auction, CancellationToken cancellationToken)
    {
        var delay = auction.EndTime - DateTime.UtcNow;

        // Never schedule in the past: if the auction already ended (or ends within
        // the next few seconds), give the transaction enough time to commit before
        // the worker tries to read the auction.
        if (delay < TimeSpan.FromSeconds(10))
        {
            delay = TimeSpan.FromSeconds(10);
        }

        var jobId = _backgroundJobService.ScheduleJob<IAuctionService>(
            repo => repo.TerminateAuctionAsync(auction.Id, cancellationToken),
            delay);

        return Task.FromResult(jobId);
    }

    public Task CancelScheduledAuction(string jobId, CancellationToken cancellationToken)
    {
        _backgroundJobService.DeleteScheduledJob(jobId);
        return Task.CompletedTask;
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
                {
                    UserId = winnerIdToNotify!,
                    Title = "مبروك! لقد فزت بالمزاد 🏆",
                    Message = $"تهانينا! لقد فزت بالمزاد على '{auction.Title}' بأعلى سعر {auction.Bids.Max(b => b.Amount):N0} ج.م.",
                    IsRead = false,
                    NotificationType = NotificationType.Auction.ToString(),
                    RelatedEntityId = auction.Id,
                    CreatedAt = DateTime.UtcNow
                };
                var notificationSeller = new NotificationDto
                {
                    UserId = sellerIdToNotify!,
                    Title = "انتهى المزاد بنجاح 💰",
                    Message = $"لقد انتهى مزادك على '{auction.Title}'. وصل أعلى سعر للمزايدة إلى {auction.Bids.Max(b => b.Amount):N0} ج.م.",
                    IsRead = false,
                    NotificationType = NotificationType.Auction.ToString(),
                    RelatedEntityId = auction.Id,
                    CreatedAt = DateTime.UtcNow
                };
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
