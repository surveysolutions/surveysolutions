using System.Web.Mvc;

namespace WB.UI.Supervisor.Models
{
    public class SupervisorUserViewModel : UserViewModel
    {
        [HiddenInput(DisplayValue = false)]
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