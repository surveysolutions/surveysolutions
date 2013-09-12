namespace Web.Supervisor.Models
{
    using System.ComponentModel.DataAnnotations;

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