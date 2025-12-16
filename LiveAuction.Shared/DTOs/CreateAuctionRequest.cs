using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace LiveAuction.Shared.DTOs;

public class CreateAuctionRequest
{
    [Required(ErrorMessage = "عنوان المزاد مطلوب")]
    [StringLength(200, ErrorMessage = "عنوان المزاد يجب أن يكون أقل من 200 حرف")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "وصف المزاد مطلوب")]
    [StringLength(1000, ErrorMessage = "وصف المزاد يجب أن يكون أقل من 1000 حرف")]
    public string Description { get; set; } = string.Empty;
    public IFormFile Image { get; set; } = null!;

    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    

    [Required(ErrorMessage = "سعر البداية مطلوب")]
    [Range(0.01, double.MaxValue, ErrorMessage = "سعر البداية يجب أن يكون أكبر من صفر")]
    public decimal StartingBid { get; set; }
    [Required(ErrorMessage = "اسم البائع مطلوب")]

    public string Seller { get; set; } = string.Empty;

    [Required(ErrorMessage = "مدة المزاد مطلوبة")]
    [Range(1, 10080, ErrorMessage = "مدة المزاد يجب أن تكون بين 1 دقيقة وأسبوع (10080 دقيقة)")]
    public int DurationInMinutes { get; set; }
}
