using Android.App;
using Android.Content.PM;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;

namespace WB.UI.QuestionnaireTester.Mvvm.Views
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize, Icon = "@drawable/icon")]
    public class InterviewView : BaseActivityView<InterviewViewModel>
    {
        protected override int ViewResourceId
        {
            get { return Resource.Layout.Interivew; }
        }
    }
}