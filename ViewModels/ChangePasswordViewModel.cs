using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class CustomPasswordValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is string password)
            {
                bool hasUpper = password.Any(char.IsUpper);
                bool hasLower = password.Any(char.IsLower);
                bool hasDigit = password.Any(char.IsDigit);
                bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));
                
                return hasUpper && hasLower && hasDigit && hasSpecial;
            }
            return false;
        }
        
        public override string FormatErrorMessage(string name)
        {
            return $"{name} must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.";
        }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "The {0} must be at {2} and at max {1} characters long.")]
        [CustomPasswordValidation]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        [Compare("ConfirmNewPassword", ErrorMessage = "Password does not match.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        public string ConfirmNewPassword { get; set; } = string.Empty;

        public string? Token { get; set; }
    }
}