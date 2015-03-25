using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Models
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    [DisplayName("Registration")]
    public class RegisterModel : PasswordModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email", Order = 4)]
        public string Email { get; set; }

        [Required]
        [Display(Name = "User name", Order = 1)]
        [RegularExpression("^[a-zA-Z0-9_]{3,15}$", ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "User_name_needs_to_be_between_3_and_15_characters")]
        public string UserName { get; set; }
    }
}