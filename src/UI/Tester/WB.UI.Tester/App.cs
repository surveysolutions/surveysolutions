using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Tester.ViewModels;

namespace WB.UI.Tester
{
    public class App : MvxApplication
    {
        public override void Initialize()
        {
            //fix for Thai calendar (KP-6403)
            var thai = new System.Globalization.ThaiBuddhistCalendar();

            RegisterAppStart<DashboardViewModel>();
        }
    }
}