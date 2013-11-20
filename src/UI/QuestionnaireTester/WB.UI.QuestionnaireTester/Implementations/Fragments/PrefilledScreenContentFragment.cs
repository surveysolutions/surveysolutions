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
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Shared.Android.Controls.ScreenItems;
using WB.UI.Shared.Android.Frames;

namespace WB.UI.QuestionnaireTester.Implementations.Fragments
{
    public class PrefilledScreenContentFragment : ScreenContentFragment
    {
        public static PrefilledScreenContentFragment CreatePrefilledScreenContentFragment(Guid questionnaireTemplateId)
        {
            var screen = new PrefilledScreenContentFragment();

            Bundle args = new Bundle();
            args.PutString(SCREEN_ID, Guid.Empty.ToString());
            args.PutString(INTERVIEW_ID, questionnaireTemplateId.ToString());
            screen.Arguments = args;

            return screen;
        }

        public PrefilledScreenContentFragment()
        {
        }

        protected override IQuestionViewFactory GetQuestionViewFactory()
        {
            return CapiTesterApplication.Kernel.Get<IQuestionViewFactory>();
        }

        protected override QuestionnaireScreenViewModel GetScreenViewModel()
        {
            var questionnaire = CapiTesterApplication.LoadView<QuestionnaireScreenInput, InterviewViewModel>(
              new QuestionnaireScreenInput(QuestionnaireId));

            return new QuestionnaireScreenViewModel(QuestionnaireId, "Pre-filled questions", "test", true, new InterviewItemId(Guid.Empty),
                questionnaire.FeaturedQuestions.Values.Select(q => q as IQuestionnaireItemViewModel).ToList(),
                new HashSet<InterviewItemId>(), new HashSet<InterviewItemId>(), 0, 0);
        }

        protected override List<IQuestionnaireViewModel> GetBreadcrumbs()
        {
            return new List<IQuestionnaireViewModel>();
        }

        protected override InterviewStatus GetInterviewStatus()
        {
            return InterviewStatus.InterviewerAssigned;
        }
    }
}