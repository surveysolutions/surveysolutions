using WB.UI.Designer.Resources;
using System.ComponentModel.DataAnnotations;

namespace WB.UI.Designer.Models
{
    public class ChangePasswordModel : PasswordModel
    {
        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "OldPassword_required")]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }
    }

    public class UpdateUserProfileModel
    {
        public string FullName { get; set; }

        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "Email_required")]
        [EmailAddress(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "InvalidEmailAddress")]
        public string Email { get; set; }
    }

    public class ManageAccountModel
    {
        public ChangePasswordModel ChangePassword { get; set; }
        public UpdateUserProfileModel UserProfile { get; set; }
    }
}
