using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.OS;
using Ninject;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Capi.Implementations.Activities;
using WB.UI.Shared.Android.Activities;
using WB.UI.Shared.Android.Controls.ScreenItems;
using WB.UI.Shared.Android.Frames;

namespace WB.UI.Capi.Implementations.Fragments
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
            return CapiApplication.Kernel.Get<IQuestionViewFactory>();
        }

        protected override QuestionnaireScreenViewModel GetScreenViewModel()
        {
            var questionnaire = CapiApplication.LoadView<QuestionnaireScreenInput, InterviewViewModel>(
                new QuestionnaireScreenInput(this.QuestionnaireId));

            if (questionnaire.FeaturedQuestions.Count == 0)
            {
                var intent = new Intent(this.Activity, typeof(DataCollectionDetailsActivity));
                intent.PutExtra("publicKey", this.QuestionnaireId.ToString());
                this.StartActivity(intent);
                this.Activity.Finish();
            }
            return new QuestionnaireScreenViewModel(this.QuestionnaireId, "Pre-filled questions", "", true, new InterviewItemId(Guid.Empty),
                questionnaire.FeaturedQuestions.Values.Select(q => q as IQuestionnaireItemViewModel).ToList(),
                new HashSet<InterviewItemId>(), new HashSet<InterviewItemId>());
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