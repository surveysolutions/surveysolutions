using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Models
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    [DisplayName("Registration")]
    public class RegisterModel : PasswordModel
    {
        public const int FullNameMaxLength = 100;

        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "Email_required")]
        [EmailAddress(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "InvalidEmailAddress")]
        [Display(Name = "Email", Order = 4)]
        public string Email { get; set; }

        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "UserName_required")]
        [Display(Name = "User name", Order = 1)]
        [RegularExpression("^[a-zA-Z0-9_]{3,15}$", ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "User_name_needs_to_be_between_3_and_15_characters")]
        public string UserName { get; set; }

        [Display(Name = "Full name", Order = 2)]
        [StringLength(FullNameMaxLength, ErrorMessageResourceName = nameof(ErrorMessages.FullNameMaxLengthError), ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessage = null)]
        public string FullName { get; set; }
    }
}
