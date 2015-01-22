using System;
using Android.App;
using Android.Content.PM;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.QuestionnaireTester.Implementations.Adapters;
using WB.UI.Shared.Android.Activities;
using WB.UI.Shared.Android.Adapters;

namespace WB.UI.QuestionnaireTester.Views
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class InterviewView : DetailsActivity
    {
        public new InterviewViewModel ViewModel
        {
            get { return (InterviewViewModel) base.ViewModel; }
            set { base.ViewModel = value; }
        }
        protected override Guid QuestionnaireId
        {
            get { return base.Model.PublicKey; }
        }

        protected override ContentFrameAdapter CreateFrameAdapter(InterviewItemId? screenId)
        {
            return new TesterContentFrameAdapter(this.SupportFragmentManager, this.ViewModel as InterviewViewModel,
                screenId);
        }

        protected override InterviewViewModel GetInterviewViewModel(Guid interviewId)
        {
            return
                CapiTesterApplication.LoadView<QuestionnaireScreenInput, InterviewViewModel>(
                    new QuestionnaireScreenInput(interviewId));
        }
    }
}