using System;
using Ninject;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.BoundedContexts.Capi.Views.Statistics;
using WB.UI.Shared.Android.Extensions;
using WB.UI.Shared.Android.Frames;

namespace WB.UI.QuestionnaireTester.Implementations.Fragments
{
    public class TesterStatisticsContentFragment : StatisticsContentFragment
    {
        protected override InterviewViewModel GetInterviewViewModel(Guid interviewId)
        {
            return CapiTesterApplication.LoadView<QuestionnaireScreenInput, InterviewViewModel>(new QuestionnaireScreenInput(interviewId));
        }

        protected override void PreCompleteAction()
        {
            base.PreCompleteAction();

            this.Activity.ClearAllBackStack<QuestionnaireListActivity>();
        }
    }
}