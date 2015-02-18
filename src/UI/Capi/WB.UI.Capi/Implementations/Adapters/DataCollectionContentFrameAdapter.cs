using System;
using Android.OS;
using Android.Support.V4.App;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Capi.Implementations.Fragments;
using WB.UI.Shared.Android.Adapters;
using WB.UI.Shared.Android.Frames;

namespace WB.UI.Capi.Implementations.Adapters
{
    public class DataCollectionContentFrameAdapter : ContentFrameAdapter
    {
        public DataCollectionContentFrameAdapter(FragmentManager fm, InterviewViewModel questionnaire, InterviewItemId? screenId)
            : base(fm, questionnaire, screenId) {}

        protected override GridContentFragment CreateRosterScreen(InterviewItemId screenId, Guid questionnaireId)
        {
            GridContentFragment myFragment = new DataCollectionGridContentFragment();

            Bundle args = new Bundle();
            args.PutString(GridContentFragment.ScreenId, ConversionHelper.ConvertIdAndRosterVectorToString(screenId.Id, screenId.InterviewItemPropagationVector));
            args.PutString(GridContentFragment.QuestionnaireId, questionnaireId.ToString());
            myFragment.Arguments = args;

            return myFragment;
        }

        protected override ScreenContentFragment CreateContentScreen(InterviewItemId screenId, Guid questionnaireId)
        {
            return DataCollectionScreenContentFragment.CreateDataCollectionScreenContentFragment(questionnaireId, screenId);
        }

        protected override StatisticsContentFragment CreateStatisticsScreen(Guid questionnaireId)
        {
            StatisticsContentFragment myFragment = new DataCollectionStatisticsContentFragment();

            Bundle args = new Bundle();
            args.PutString(StatisticsContentFragment.QUESTIONNAIRE_ID, questionnaireId.ToString());
            myFragment.Arguments = args;

            return myFragment;
        }
    }
}