using System.ComponentModel.DataAnnotations;

namespace WB.UI.Headquarters.Models.Team
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
        public new string Password
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