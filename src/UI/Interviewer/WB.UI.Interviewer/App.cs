using Cirrious.MvvmCross.ViewModels;

namespace WB.UI.Interviewer
{
    public class App : MvxApplication
    {
        public override void Initialize()
        {
            //fix for Thai calendar (KP-6403)
            var thai = new System.Globalization.ThaiBuddhistCalendar();
        }
    }
}