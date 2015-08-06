using Android.Content;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Droid.Platform;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.UI.Capi.ViewModel
{
    public class InterviewerPrefilledQuestionsViewModel : PrefilledQuestionsViewModel
    {
        public InterviewerPrefilledQuestionsViewModel(IInterviewViewModelFactory interviewViewModelFactory,
            IPlainKeyValueStorage<QuestionnaireModel> plainQuestionnaireRepository,
            IStatefulInterviewRepository interviewRepository, 
            IViewModelNavigationService viewModelNavigationService) : 
            base(interviewViewModelFactory, plainQuestionnaireRepository, interviewRepository, viewModelNavigationService)
        {
        }

        public override void NavigateToPreviousViewModel()
        {
            var mvxAndroidCurrentTopActivity = Mvx.Resolve<IMvxAndroidCurrentTopActivity>();
            var intent = new Intent(mvxAndroidCurrentTopActivity.Activity, typeof(DashboardActivity));
            intent.AddFlags(ActivityFlags.NoHistory);
            mvxAndroidCurrentTopActivity.Activity.StartActivity(intent);
        }
    }
}