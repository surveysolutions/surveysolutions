using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;

namespace WB.UI.Interviewer
{
    public class App : MvxApplication
    {
        public override void Initialize()
        {
            base.Initialize();
            Mvx.Resolve<InterviewerDashboardEventHandler>(); // In order to start listening for interview events
        }
    }
}