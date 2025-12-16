using System.ComponentModel.DataAnnotations;

namespace LiveAuction.Shared.DTOs;

public class UpdateAuctionRequest
{
    [Required(ErrorMessage = "⁄‰Ê«‰ «·„“«œ „ÿ·Ê»")]
    [StringLength(200, ErrorMessage = "⁄‰Ê«‰ «·„“«œ ÌÃ» √‰ ÌﬂÊ‰ √ﬁ· „‰ 200 Õ—›")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ê’› «·„“«œ „ÿ·Ê»")]
    [StringLength(1000, ErrorMessage = "Ê’› «·„“«œ ÌÃ» √‰ ÌﬂÊ‰ √ﬁ· „‰ 1000 Õ—›")]
    public string Description { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }
}