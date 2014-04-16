using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Web.Supervisor.Models
{
    public class UserViewModelForSupervisor : UserViewModel
    {
        [ReadOnly(true)]
        [Display(Name = "Is Locked by Headquarters", Order = 6)]
        public new bool IsLockedByHQ
        {
            get
            {
                return base.IsLockedByHQ;
            }
            set
            {
                base.IsLockedByHQ = value;
            }
        }
    }
}