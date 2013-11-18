using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Shared.Android.Controls.ScreenItems;
using WB.UI.Shared.Android.Frames;

namespace WB.UI.Capi.Implementations.Fragments
{
    public class DataCollectionScreenContentFragment : ScreenContentFragment
    {
        protected override IQuestionViewFactory GetQuestionViewFactory()
        {
            return CapiApplication.Kernel.Get<IQuestionViewFactory>();
        }

        protected override QuestionnaireScreenViewModel GetScreenViewModel()
        {
            return this.Questionnaire.Screens[ScreenId] as QuestionnaireScreenViewModel;
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
                if (this.questionnaire == null)
                {
                    this.questionnaire =
                        CapiApplication.LoadView<QuestionnaireScreenInput, InterviewViewModel>(
                            new QuestionnaireScreenInput(QuestionnaireId));
                }
                return this.questionnaire;
            }
        }
    }
}