using MvvmCross.ViewModels;

namespace WB.UI.Supervisor
{
    public class SupervisorAppStart : MvxAppStart
    {
        public SupervisorAppStart(IMvxApplication application) : base(application)
        {
        }

        protected override void Startup(object hint = null)
        {
            base.ApplicationStartup(hint);
        }
    }
}
