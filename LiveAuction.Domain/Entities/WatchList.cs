namespace LiveAuction.Domain.Entities;

public class WatchList
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ICollection<WatchListItem> Items { get; set; } = [];
    public ApplicationUser User { get; set; } = null!;
}
