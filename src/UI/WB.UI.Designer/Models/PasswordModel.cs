using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Models
{
    using System.ComponentModel.DataAnnotations;

    public class PasswordModel
    {
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password", Order = 3)]
        [Compare("Password", ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "The_password_and_confirmation_password_do_not_match")]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "Password_required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password", Order = 2)]
        public string? Password { get; set; }
    }
}
