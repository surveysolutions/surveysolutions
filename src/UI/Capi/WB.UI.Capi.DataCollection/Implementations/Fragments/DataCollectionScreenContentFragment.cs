using System;
using Ninject;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.UI.Shared.Android.Controls.ScreenItems;
using WB.UI.Shared.Android.Frames;

namespace WB.UI.Capi.DataCollection.Implementations.Fragments
{
    public class DataCollectionScreenContentFragment : ScreenContentFragment
    {
        protected override IQuestionViewFactory GetQuestionViewFactory()
        {
            return CapiApplication.Kernel.Get<IQuestionViewFactory>();
        }

        protected override InterviewViewModel GetInterviewViewModel(Guid questionnaireId)
        {
            return CapiApplication.LoadView<QuestionnaireScreenInput, InterviewViewModel>(new QuestionnaireScreenInput(questionnaireId));
        }
    }
}