using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Shared.Android.Controls.ScreenItems;
using WB.UI.Shared.Android.Frames;

namespace WB.UI.QuestionnaireTester.Implementations.Fragments
{
    public class TesterScreenContentFragment : ScreenContentFragment
    {
        public TesterScreenContentFragment()
        {
        }

        protected override IQuestionViewFactory GetQuestionViewFactory()
        {
            return CapiTesterApplication.Kernel.Get<IQuestionViewFactory>();
        }

        protected override QuestionnaireScreenViewModel GetScreenViewModel()
        {
            return Questionnaire.Screens[ScreenId] as QuestionnaireScreenViewModel;
        }

        protected override List<IQuestionnaireViewModel> GetBreadcrumbs()
        {
            return Questionnaire.RestoreBreadCrumbs(this.GetScreenViewModel().Breadcrumbs).ToList();
        }

        protected override InterviewStatus GetStatus()
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
                    questionnaire = CapiTesterApplication.LoadView<QuestionnaireScreenInput, InterviewViewModel>(
                        new QuestionnaireScreenInput(QuestionnaireId));
                }
                return questionnaire;
            }
        }
    }
}