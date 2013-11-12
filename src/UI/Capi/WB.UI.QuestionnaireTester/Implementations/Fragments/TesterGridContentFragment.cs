using System;
using Ninject;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.UI.Shared.Android.Controls.ScreenItems;
using WB.UI.Shared.Android.Frames;

namespace WB.UI.Capi.Tester.Implementations.Fragments
{
    public class TesterGridContentFragment : GridContentFragment
    {
        protected override IQuestionViewFactory GetQuestionViewFactory()
        {
            return CapiTesterApplication.Kernel.Get<IQuestionViewFactory>();
        }

        protected override InterviewViewModel GetInterviewViewModel(Guid questionnaireId)
        {
            return CapiTesterApplication.LoadView<QuestionnaireScreenInput, InterviewViewModel>(new QuestionnaireScreenInput(questionnaireId));
        }
    }
}