using Android.App;
using Android.Content.PM;
using Android.Views;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;

namespace WB.UI.QuestionnaireTester.Mvvm.Views
{
    [Activity(WindowSoftInputMode = SoftInput.AdjustPan,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize, Theme = "@style/Theme.Tester")]
    public class QuestionnairePrefilledQuestionsView : BaseActivityView<QuestionnairePrefilledQuestionsViewModel>
    {
        protected override int ViewResourceId
        {
            get { return Resource.Layout.QuestionnairePrefilledQuestionsView; }
        }
    }
}