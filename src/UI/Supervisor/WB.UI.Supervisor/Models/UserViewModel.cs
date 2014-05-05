namespace Web.Supervisor.Models
{
    using System.ComponentModel;

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