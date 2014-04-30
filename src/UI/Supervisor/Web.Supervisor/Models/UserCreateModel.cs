using System.ComponentModel;
using System.Web.Mvc;

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

        [HiddenInput(DisplayValue = false)]
        [Display(Order = 5)]
        public new bool IsLockedBySupervisor
        {
            get
            {
                return base.IsLockedBySupervisor;
            }
            set
            {
                base.IsLockedBySupervisor = value;
            }
        }
       
    }
}