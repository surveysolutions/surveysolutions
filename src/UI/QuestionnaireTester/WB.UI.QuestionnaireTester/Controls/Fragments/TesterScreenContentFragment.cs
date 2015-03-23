using System;
using System.Collections.Generic;
using System.Linq;
using Android.OS;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Shared.Android.Controls.ScreenItems;
using WB.UI.Shared.Android.Frames;

namespace WB.UI.QuestionnaireTester.Controls.Fragments
{
    public class TesterScreenContentFragment : ScreenContentFragment
    {
        public static TesterScreenContentFragment CreateTesterScreenContentFragment(Guid interviewId, InterviewItemId screenId)
        {
            var testerScreenContentFragment = new TesterScreenContentFragment();

            Bundle args = new Bundle();
            args.PutString(ScreenContentFragment.SCREEN_ID, screenId.ToString());
            args.PutString(ScreenContentFragment.INTERVIEW_ID, interviewId.ToString());
            testerScreenContentFragment.Arguments = args;

            return testerScreenContentFragment;
        }

        public TesterScreenContentFragment()
        {
        }

        protected override IQuestionViewFactory GetQuestionViewFactory()
        {
            return ServiceLocator.Current.GetInstance<IQuestionViewFactory>();
        }

        protected override QuestionnaireScreenViewModel GetScreenViewModel()
        {
            return Questionnaire.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(ScreenId.Id, ScreenId.InterviewItemPropagationVector)] as QuestionnaireScreenViewModel;
        }

        protected override List<IQuestionnaireViewModel> GetBreadcrumbs()
        {
            return Questionnaire.RestoreBreadCrumbs(this.GetScreenViewModel().Breadcrumbs).ToList();
        }

        protected override InterviewStatus GetInterviewStatus()
        {
            return Questionnaire.Status;
        }



        private InterviewViewModel questionnaire;

        protected InterviewViewModel Questionnaire
        {
            get
            {
                if (questionnaire == null)
                {
                    questionnaire = ServiceLocator.Current.GetInstance<IViewFactory<QuestionnaireScreenInput, InterviewViewModel>>()
                    .Load(new QuestionnaireScreenInput(QuestionnaireId));
                }
                return questionnaire;
            }
        }
    }
}