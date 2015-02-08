using System;
using Microsoft.Practices.ServiceLocation;
using Ninject;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Shared.Android.Extensions;
using WB.UI.Shared.Android.Frames;

namespace WB.UI.QuestionnaireTester.Implementations.Fragments
{
    public class TesterStatisticsContentFragment : StatisticsContentFragment
    {
        protected override InterviewViewModel GetInterviewViewModel(Guid interviewId)
        {
            return ServiceLocator.Current.GetInstance<IViewFactory<QuestionnaireScreenInput, InterviewViewModel>>()
                    .Load(new QuestionnaireScreenInput(interviewId));
        }

        protected override void PreCompleteAction()
        {
            base.PreCompleteAction();

            //this.Activity.ClearAllBackStack<QuestionnaireListActivity>();
        }
    }
}