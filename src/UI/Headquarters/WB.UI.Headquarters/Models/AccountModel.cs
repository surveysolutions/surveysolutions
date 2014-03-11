using System.ComponentModel.DataAnnotations;

namespace WB.UI.Headquarters.Models
{
    public class AccountModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool AdminRoleEnabled { get; set; }

        public bool HeadquarterRoleEnabled { get; set; }

        public bool UserNameChangeAllowed { get; set; }

        public string Id { get; set; }
    }
}