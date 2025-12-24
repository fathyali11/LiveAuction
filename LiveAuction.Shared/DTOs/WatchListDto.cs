namespace LiveAuction.Shared.DTOs;

public class WatchListDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public List<ToggleWatchListItemResponse> Items { get; set; } = [];
}