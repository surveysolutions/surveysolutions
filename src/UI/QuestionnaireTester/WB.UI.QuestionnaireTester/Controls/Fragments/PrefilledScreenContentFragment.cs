using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.OS;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.QuestionnaireTester.Mvvm.Views;
using WB.UI.Shared.Android.Controls.ScreenItems;
using WB.UI.Shared.Android.Frames;

namespace WB.UI.QuestionnaireTester.Controls.Fragments
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
            return ServiceLocator.Current.GetInstance<IQuestionViewFactory>();
        }

        protected override QuestionnaireScreenViewModel GetScreenViewModel()
        {
            var questionnaire =
                ServiceLocator.Current.GetInstance<IViewFactory<QuestionnaireScreenInput, InterviewViewModel>>()
                    .Load(new QuestionnaireScreenInput(QuestionnaireId));

            if (questionnaire.FeaturedQuestions.Count == 0)
            {
                var intent = new Intent(this.Activity, typeof (InterviewView));
                intent.PutExtra("publicKey", QuestionnaireId.ToString());
                this.StartActivity(intent);
                this.Activity.Finish();
            }
            return new QuestionnaireScreenViewModel(QuestionnaireId, "Pre-filled questions", "test", true, new InterviewItemId(Guid.Empty),
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