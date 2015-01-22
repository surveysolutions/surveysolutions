using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using WB.UI.QuestionnaireTester.Implementations.Fragments;
using WB.UI.QuestionnaireTester.ViewModels;

namespace WB.UI.QuestionnaireTester.Views
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
        }
    }
}