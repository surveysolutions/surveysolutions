using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.UI.Interviewer.ViewModel;

namespace WB.UI.Interviewer
{
    public class App : MvxApplication
    {
        public override void Initialize()
        {
            this.RegisterAppStart<DashboardViewModel>();
        }
    }
}