using LiveAuction.Shared.DTOs;
using Refit;

namespace LiveAuction.Client.Features.Auctions.Services;

public interface IAuctionsApi 
{
    [Get("/api/auctions")]
    Task<List<AuctionsInHomePageDto>> GetAllAsync(CancellationToken cancellationToken = default);
    
    [Get("/api/auctions/{id}")]
    Task<AuctionDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    
    [Post("/api/auctions")]
    Task<HttpResponseMessage> CreateAsync([Body] MultipartFormDataContent content, CancellationToken cancellationToken = default);
    
    [Put("/api/auctions/{id}")]
    Task<HttpResponseMessage> UpdateAsync(int id, [Body] UpdateAuctionRequest request, CancellationToken cancellationToken = default);
    
    [Delete("/api/auctions/{id}")]
    Task<HttpResponseMessage> DeleteAsync(int id, CancellationToken cancellationToken = default);
}