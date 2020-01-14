using System;
using System.ComponentModel.DataAnnotations;

namespace WB.UI.Headquarters.Models.Users
{
    public class ChangePasswordModel
    {
        public Guid UserId { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [DataType(DataType.Password)]
        public string OldPassword { get; set; }
    }
}
