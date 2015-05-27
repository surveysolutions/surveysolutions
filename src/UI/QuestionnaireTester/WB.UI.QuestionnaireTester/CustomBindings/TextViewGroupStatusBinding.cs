using System;
using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Widget;
using Cirrious.CrossCore.Droid.Platform;
using Cirrious.MvvmCross.Binding;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.QuestionnaireTester.Properties;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.GenericSubdomains.Portable;


namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class TextViewGroupStatusBinding : BaseBinding<TextView, GroupStatistics>
    {
        public TextViewGroupStatusBinding(TextView androidControl)
            : base(androidControl)
        {
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        } 

        protected override void SetValueToView(TextView control, GroupStatistics value)
        {
            if (value == null) return;

            var spannableText = value.UnansweredQuestionsCount == 0 
                ? this.GetSpannableStringForAnsweredState(value) 
                : this.GetSpannableStringForUnansweredState(value);

            control.TextFormatted = spannableText;
        }

        private SpannableString GetSpannableStringForUnansweredState(GroupStatistics statistics)
        {
            var unansweredQuestionsFormatString = UIResources.Interview_PreviousGroupNavigation_UnansweredQuestions;
            var groupStatisticsText = unansweredQuestionsFormatString.FormatString(statistics.UnansweredQuestionsCount);
            groupStatisticsText = groupStatisticsText.ToUpper();

            var paternToInsertUnansweredCount = "{0}";
            var indexOfStartAnswersCount = unansweredQuestionsFormatString.IndexOf(paternToInsertUnansweredCount, StringComparison.InvariantCultureIgnoreCase);
            var countCharsAfterFormatMask = unansweredQuestionsFormatString.Length - indexOfStartAnswersCount - paternToInsertUnansweredCount.Length;
            var indexOfEndAnswersCount = groupStatisticsText.Length - countCharsAfterFormatMask;
            
            var spannableText = new SpannableString(groupStatisticsText);
            var unansweredColor = this.GetColorFromResources(Resource.Color.previous_group_navigation_unanswered);
            spannableText.SetSpan(new ForegroundColorSpan(unansweredColor), indexOfStartAnswersCount, indexOfEndAnswersCount, SpanTypes.ExclusiveExclusive);
            return spannableText;
        }

        private SpannableString GetSpannableStringForAnsweredState(GroupStatistics statistics)
        {
            var groupStatisticsText = UIResources.Interview_PreviousGroupNavigation_AnsweredQuestions.FormatString(statistics.QuestionsCount);
            groupStatisticsText = groupStatisticsText.ToUpper();
            var spannableText = new SpannableString(groupStatisticsText);
            var answeredColor = this.GetColorFromResources(Resource.Color.previous_group_navigation_answered);
            spannableText.SetSpan(new ForegroundColorSpan(answeredColor), 0, spannableText.Length(), SpanTypes.ExclusiveExclusive);
            return spannableText;
        }

        private Color GetColorFromResources(int resourceId)
        {
            return Target.Resources.GetColor(resourceId);
        }
    }
}