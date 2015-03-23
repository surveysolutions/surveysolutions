using System;
using Android.App;
using Android.Content.PM;
using Cirrious.CrossCore;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.QuestionnaireTester.Controls.Adapters;
using WB.UI.Shared.Android.Activities;
using WB.UI.Shared.Android.Adapters;
using WB.UI.QuestionnaireTester.Extensions;

namespace WB.UI.QuestionnaireTester.Mvvm.Views
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize, Icon = "@drawable/icon")]
    public class InterviewView : DetailsActivity
    {
        protected override ContentFrameAdapter CreateFrameAdapter(InterviewItemId? screenId)
        {
            return new TesterContentFrameAdapter(this.SupportFragmentManager, this.ViewModel as InterviewViewModel,
                screenId);
        }

        protected override InterviewViewModel GetInterviewViewModel(Guid interviewId)
        {
            return
                ServiceLocator.Current.GetInstance<IViewFactory<QuestionnaireScreenInput, InterviewViewModel>>()
                    .Load(new QuestionnaireScreenInput(interviewId));
        }

        protected override void OnStart()
        {
            base.OnStart();
            this.CreateActionBar(Mvx.Resolve<IAnswerProgressIndicator>());
        }
    }
}