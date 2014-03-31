using System.ComponentModel;

namespace WB.UI.Headquarters.Models.Team
{
    public class UserViewModel : UserEditModel
    {
        [ReadOnly(true)]
        public new string UserName {
            get
            {
                return base.UserName;
            }
            set
            {
                base.UserName = value;
            }
        }
    }
}