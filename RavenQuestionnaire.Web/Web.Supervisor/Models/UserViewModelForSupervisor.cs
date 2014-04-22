using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Web.Supervisor.Models
{
    public class UserViewModelForSupervisor : UserViewModel
    {
        [ReadOnly(true)]
        [Display(Order = 6)]
        public new bool IsLockedByHeadquarters
        {
            get
            {
                return base.IsLockedByHeadquarters;
            }
            set
            {
                base.IsLockedByHeadquarters = value;
            }
        }
    }
}