using System;
using Android.OS;
using Android.Support.V4.App;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.QuestionnaireTester.Implementations.Fragments;
using WB.UI.Shared.Android.Adapters;
using WB.UI.Shared.Android.Frames;

namespace WB.UI.QuestionnaireTester.Implementations.Adapters
{
    public class TesterContentFrameAdapter : ContentFrameAdapter
    {
        public TesterContentFrameAdapter(FragmentManager fm, InterviewViewModel questionnaire, InterviewItemId? screenId)
            : base(fm, questionnaire, screenId) {}

        protected override GridContentFragment CreateRosterScreen(InterviewItemId screenId, Guid questionnaireId)
        {
            GridContentFragment myFragment = new TesterGridContentFragment();

            Bundle args = new Bundle();
            args.PutString(GridContentFragment.ScreenId, ConversionHelper.ConvertIdAndRosterVectorToString(screenId.Id, screenId.InterviewItemPropagationVector));
            args.PutString(GridContentFragment.QuestionnaireId, questionnaireId.ToString());
            myFragment.Arguments = args;

            return myFragment;
        }

        protected override ScreenContentFragment CreateContentScreen(InterviewItemId screenId, Guid questionnaireId)
        {
            return TesterScreenContentFragment.CreateTesterScreenContentFragment(questionnaireId, screenId);
        }

        protected override StatisticsContentFragment CreateStatisticsScreen(Guid questionnaireId)
        {
            StatisticsContentFragment myFragment = new TesterStatisticsContentFragment();

            Bundle args = new Bundle();
            args.PutString(StatisticsContentFragment.QUESTIONNAIRE_ID, questionnaireId.ToString());
            myFragment.Arguments = args;

            return myFragment;
        }
    }
}