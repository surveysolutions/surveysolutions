using System;
using Microsoft.Practices.ServiceLocation;
using Ninject;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.Infrastructure.ReadSide;
using WB.UI.Shared.Android.Controls.ScreenItems;
using WB.UI.Shared.Android.Frames;

namespace WB.UI.QuestionnaireTester.Implementations.Fragments
{
    public class TesterGridContentFragment : GridContentFragment
    {
        protected override IQuestionViewFactory GetQuestionViewFactory()
        {
            return ServiceLocator.Current.GetInstance<IQuestionViewFactory>();
        }

        protected override InterviewViewModel GetInterviewViewModel(Guid questionnaireId)
        {
            return ServiceLocator.Current.GetInstance<IViewFactory<QuestionnaireScreenInput, InterviewViewModel>>()
                    .Load(new QuestionnaireScreenInput(questionnaireId));
        }
    }
}