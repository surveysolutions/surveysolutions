using System.ComponentModel.DataAnnotations;

namespace WB.UI.Headquarters.Models.Users
{
    public class ChangePasswordModel
    {
        [DataType(DataType.Password)]
        [Required]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Required]
        public string ConfirmPassword { get; set; }

        [DataType(DataType.Password)]
        [Required]
        public string OldPassword { get; set; }
    }
}
