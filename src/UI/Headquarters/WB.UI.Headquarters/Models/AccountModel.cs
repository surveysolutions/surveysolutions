using System.ComponentModel.DataAnnotations;
using WB.UI.Headquarters.Resources.Users;

namespace WB.UI.Headquarters.Models
{
    public abstract class AccountModel
    {
        [Required(ErrorMessageResourceType = typeof(UsersResources), ErrorMessageResourceName = "UserNameRequired")]
        [Display(ResourceType = typeof(UsersResources), Name = "UserName")]
        [RegularExpression("^[a-zA-Z0-9_]{3,15}$", ErrorMessageResourceType = typeof(UsersResources), ErrorMessageResourceName = "InvalidUserName")]
        public string UserName { get; set; }


        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(UsersResources), Name = "Password")]
        public virtual string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessageResourceType = typeof(UsersResources), ErrorMessageResourceName = "ConfirmPasswordMustMatch")]
        [Display(ResourceType = typeof(UsersResources), Name = "ConfirmPassword")]
        public string PasswordConfirm { get; set; }

        public bool AdminRoleEnabled { get; set; }

        public bool HeadquarterRoleEnabled { get; set; }

        public string Id { get; set; }

    }
}