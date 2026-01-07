namespace LiveAuction.Shared.DTOs;

public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; } 
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
public class PaginatedRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; }= 10;
    public string? SearchTerm { get; set; }
}