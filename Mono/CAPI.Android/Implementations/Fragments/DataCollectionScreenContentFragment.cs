using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Ninject;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.UI.Capi.Shared.Controls.ScreenItems;
using WB.UI.Capi.Shared.Frames;

namespace CAPI.Android.Implementations.Fragments
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