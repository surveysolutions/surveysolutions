using System;
using System.Collections.Generic;
using System.Linq;
using Android.OS;
using Ninject;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
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
            return this.Interview.Screens[ScreenId] as QuestionnaireScreenViewModel;
        }

        protected override List<IQuestionnaireViewModel> GetBreadcrumbs()
        {
            var currentScreen = this.GetScreenViewModel();
            if (currentScreen == null)
                throw new NullReferenceException("Screen is missing inside interview");

            return this.Interview.RestoreBreadCrumbs(currentScreen.Breadcrumbs).ToList();
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