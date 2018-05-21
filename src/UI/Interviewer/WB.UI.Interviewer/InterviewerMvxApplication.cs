using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.ViewModels;

namespace WB.UI.Interviewer
{
    public class InterviewerMvxApplication : MvxApplication
    {
        public override void Initialize()
        {
            base.Initialize();
            RegisterCustomAppStart<InterviewerAppStart>();
        }
    }
}
