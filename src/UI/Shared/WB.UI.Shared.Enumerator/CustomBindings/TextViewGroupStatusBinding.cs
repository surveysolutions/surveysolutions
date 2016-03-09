﻿using System;
using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Widget;
using MvvmCross.Binding;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class TextViewGroupStatusBinding : BaseBinding<TextView, GroupNavigationViewModel.GroupStatistics>
    {
        public TextViewGroupStatusBinding(TextView androidControl)
            : base(androidControl)
        {
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        } 

        protected override void SetValueToView(TextView control, GroupNavigationViewModel.GroupStatistics value)
        {
            if (value == null) return;

            var spannableText = value.UnansweredQuestionsCount == 0 
                ? this.GetSpannableStringForAnsweredState(value) 
                : this.GetSpannableStringForUnansweredState(value);

            control.TextFormatted = spannableText;
        }

        private SpannableString GetSpannableStringForUnansweredState(GroupNavigationViewModel.GroupStatistics statistics)
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

        private SpannableString GetSpannableStringForAnsweredState(GroupNavigationViewModel.GroupStatistics statistics)
        {
            var groupStatisticsText = UIResources.Interview_PreviousGroupNavigation_AnsweredQuestions.FormatString(statistics.EnabledQuestionsCount);
            groupStatisticsText = groupStatisticsText.ToUpper();
            var spannableText = new SpannableString(groupStatisticsText);
            var answeredColor = this.GetColorFromResources(Resource.Color.previous_group_navigation_answered);
            spannableText.SetSpan(new ForegroundColorSpan(answeredColor), 0, spannableText.Length(), SpanTypes.ExclusiveExclusive);
            return spannableText;
        }

        private Color GetColorFromResources(int resourceId)
        {
            return this.Target.Resources.GetColor(resourceId);
        }
    }
}