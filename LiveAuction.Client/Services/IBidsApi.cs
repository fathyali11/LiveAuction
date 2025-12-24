using LiveAuction.Shared.DTOs;
using Refit;

namespace LiveAuction.Client.Services;

public interface IBidsApi 
{
    [Get("/api/bids/user-history")]
    Task<List<UserBidDto>> GetUserHistoryAsync(CancellationToken cancellationToken = default);
    
    [Post("/api/bids")]
    Task<HttpResponseMessage> PlaceBidAsync([Body] CreateBidRequest request, CancellationToken cancellationToken = default);
}