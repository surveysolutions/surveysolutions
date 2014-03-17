using System.ComponentModel.DataAnnotations;
using WB.UI.Headquarters.Resources.Users;

namespace WB.UI.Headquarters.Models
{
    public abstract class AccountModel
    {
        [Required]
        [Display(ResourceType = typeof(UsersResources), Name = "UserName")]
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