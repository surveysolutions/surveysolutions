using MvvmCross.ViewModels;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Tester
{
    public class App : MvxApplication
    {
        public override void Initialize()
        {
            base.Initialize();
            //fix for Thai calendar (KP-6403)
            var thai = new System.Globalization.ThaiBuddhistCalendar();
        }
    }
}
