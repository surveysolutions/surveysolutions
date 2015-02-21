using System;
using System.Collections.Generic;
using System.Linq;
using Android.OS;
using Ninject;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Shared.Android.Controls.ScreenItems;
using WB.UI.Shared.Android.Frames;

namespace WB.UI.Capi.Implementations.Fragments
{
    public class DataCollectionScreenContentFragment : ScreenContentFragment
    {
        public static DataCollectionScreenContentFragment CreateDataCollectionScreenContentFragment(Guid interviewId,
            InterviewItemId screenId)
        {
            var dataCollectionScreenContentFragment = new DataCollectionScreenContentFragment();

            Bundle args = new Bundle();
            args.PutString(ScreenContentFragment.SCREEN_ID, screenId.ToString());
            args.PutString(ScreenContentFragment.INTERVIEW_ID, interviewId.ToString());
            dataCollectionScreenContentFragment.Arguments = args;

            return dataCollectionScreenContentFragment;
        }

        protected override IQuestionViewFactory GetQuestionViewFactory()
        {
            return CapiApplication.Kernel.Get<IQuestionViewFactory>();
        }

        protected override QuestionnaireScreenViewModel GetScreenViewModel()
        {
            var screenKey = ConversionHelper.ConvertIdAndRosterVectorToString(ScreenId.Id, ScreenId.InterviewItemPropagationVector);

            if (!this.Interview.Screens.ContainsKey(screenKey))
                throw new NullReferenceException("Screen is missing inside interview");

            var questionnaireScreenViewModel = this.Interview.Screens[screenKey] as QuestionnaireScreenViewModel;
            if (questionnaireScreenViewModel == null)
                throw new InvalidOperationException(string.Format("Screen with id {0} is {1}, but must be {2}",
                    ScreenId,
                    Interview.Screens[screenKey].GetType().Name, typeof(QuestionnaireScreenViewModel).Name));
            return questionnaireScreenViewModel;
        }

        protected override List<IQuestionnaireViewModel> GetBreadcrumbs()
        {
            return this.Interview.RestoreBreadCrumbs(GetScreenViewModel().Breadcrumbs).ToList();
        }

        protected override InterviewStatus GetInterviewStatus()
        {
            return this.Interview.Status;
        }

        private InterviewViewModel interview;

        protected InterviewViewModel Interview
        {
            get
            {
                if (this.interview == null)
                {
                    this.interview =
                        CapiApplication.LoadView<QuestionnaireScreenInput, InterviewViewModel>(
                            new QuestionnaireScreenInput(QuestionnaireId));
                }
                return this.interview;
            }
        }
    }
}