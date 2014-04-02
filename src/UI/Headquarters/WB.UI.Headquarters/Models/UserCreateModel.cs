using System.ComponentModel.DataAnnotations;

namespace WB.UI.Headquarters.Models
{
    public class UserCreateModel : UserEditModel
    {
        [Required]
        public new string UserName
        {
            get
            {
                return base.UserName;
            }
            set
            {
                base.UserName = value;
            }
        }

        [Required]
        public string Password
        {
            get
            {
                return base.Password;
            }
            set
            {
                base.Password = value;
            }
        }
    }
}