using System.ComponentModel.DataAnnotations;
using WB.UI.Headquarters.Resources.Users;

namespace WB.UI.Headquarters.Models
{
    public class AccountModel
    {
        [Required]
        [Display(ResourceType = typeof(UsersResources), Name = "UserName")]
        public string UserName { get; set; }

        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(UsersResources), Name = "Password")]
        public string Password { get; set; }

        public bool AdminRoleEnabled { get; set; }

        public bool HeadquarterRoleEnabled { get; set; }

        public bool UserNameChangeAllowed { get; set; }

        public string Id { get; set; }
    }
}