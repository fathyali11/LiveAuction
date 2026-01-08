namespace LiveAuction.Shared.DTOs;

public class PaginatedRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; }= 8;
    public string? SearchTerm { get; set; }
}