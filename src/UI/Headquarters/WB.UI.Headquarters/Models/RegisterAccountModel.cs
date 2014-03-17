using System.ComponentModel.DataAnnotations;
using WB.UI.Headquarters.Resources.Users;

namespace WB.UI.Headquarters.Models
{
    public class RegisterAccountModel : AccountModel
    {
        [Required(ErrorMessageResourceType = typeof(UsersResources), ErrorMessageResourceName = "PasswordRequired")]
        public override string Password { get; set; }
    }
}