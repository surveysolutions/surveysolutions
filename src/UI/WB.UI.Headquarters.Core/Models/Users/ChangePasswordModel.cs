using System.ComponentModel.DataAnnotations;

namespace WB.UI.Headquarters.Models.Users
{
    public class ChangePasswordModel
    {
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [DataType(DataType.Password)]
        public string OldPassword { get; set; }
    }
}
