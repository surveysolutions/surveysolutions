using MvvmCross.ViewModels;

namespace WB.UI.Supervisor
{
    public class SupervisorMvxApplication : MvxApplication
    {
        public override void Initialize()
        {
            base.Initialize();
            RegisterCustomAppStart<SupervisorAppStart>();
        }
    }
}
