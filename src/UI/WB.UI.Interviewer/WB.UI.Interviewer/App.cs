using WB.UI.Interviewer.View;
using Xamarin.Forms;

namespace WB.UI.Interviewer
{
    public class App : Application
    {
        private static ViewModelLocator _locator;
        public static ViewModelLocator Locator
        {
            get { return _locator ?? (_locator = new ViewModelLocator()); }
        }

        public static Page GetInverviewDetails()
        {
            return new InterviewDetails();
        }
    }
}
