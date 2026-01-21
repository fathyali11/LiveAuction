using System.ComponentModel.DataAnnotations;

namespace LiveAuction.Shared.DTOs;

public class RegisterRequest
{
    [Required(ErrorMessage = "الاسم بالكامل مطلوب")]
    [StringLength(100, ErrorMessage = "الاسم يجب ألا يتجاوز 100 حرف")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
    [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور مطلوبة")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*\W)\S{8,}$",
        ErrorMessage = "يجب أن تحتوي كلمة المرور على حرف كبير، وصغير، ورقم، ورمز.")]
    public string Password { get; set; } = string.Empty;
}
