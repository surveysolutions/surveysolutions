using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.UI.QuestionnaireTester.Controls.Fragments;

namespace WB.UI.QuestionnaireTester.Mvvm.Views
{
    [Activity(WindowSoftInputMode = SoftInput.AdjustPan,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize, Theme = "@style/Theme.Tester")]
    public class QuestionnairePrefilledQuestionsView : BaseFragmentActivityView<QuestionnairePrefilledQuestionsViewModel>
    {
        protected override int ViewResourceId
        {
            get { return Resource.Layout.QuestionnairePrefilledQuestionsView; }
        }
        
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.ViewModel.OnInterviewCreated += (interviewId) =>
            {
                var screen = PrefilledScreenContentFragment.CreatePrefilledScreenContentFragment(interviewId);

                SupportFragmentManager.BeginTransaction().Add(Resource.Id.flFragmentHolder, screen).Commit();  
            };

            this.ViewModel.OnInterviewDetailsOpened += (interviewId) =>
            {
                var intent = new Intent(this, typeof(InterviewView));
                intent.SetFlags(ActivityFlags.ReorderToFront);
                intent.PutExtra("publicKey", interviewId.ToString());
                this.StartActivity(intent);
            };
        }
    }
}