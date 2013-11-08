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
using CAPI.Android.Implementations.Fragments;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Shared.Android.Adapters;
using WB.UI.Shared.Android.Frames;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace CAPI.Android.Implementations.Adapters
{
    public class DataCollectionContentFrameAdapter : ContentFrameAdapter
    {
        public DataCollectionContentFrameAdapter(FragmentManager fm, InterviewViewModel questionnaire, InterviewItemId? screenId)
            : base(fm, questionnaire, screenId) {}

        protected override GridContentFragment CreateRosterScreen(InterviewItemId screenId, Guid questionnaireId)
        {
            GridContentFragment myFragment = new DataCollectionGridContentFragment();

            Bundle args = new Bundle();
            args.PutString(GridContentFragment.ScreenId, screenId.ToString());
            args.PutString(GridContentFragment.QuestionnaireId, questionnaireId.ToString());
            myFragment.Arguments = args;

            return myFragment;
        }

        protected override ScreenContentFragment CreateContentScreen(InterviewItemId screenId, Guid questionnaireId)
        {
            ScreenContentFragment myFragment = new DataCollectionScreenContentFragment();

            Bundle args = new Bundle();
            args.PutString(ScreenContentFragment.SCREEN_ID, screenId.ToString());
            args.PutString(ScreenContentFragment.QUESTIONNAIRE_ID, questionnaireId.ToString());
            myFragment.Arguments = args;

            return myFragment;
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