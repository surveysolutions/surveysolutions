using System;
using System.ComponentModel;
using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Widget;
using Cirrious.CrossCore.Droid.Platform;
using Cirrious.MvvmCross.Binding;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.QuestionnaireTester.Properties;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;

namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class TextViewGroupInfoBinding : BaseBinding<TextView, GroupState>
    {
        private IMvxAndroidCurrentTopActivity CurrentTopActivity
        {
            get { return ServiceLocator.Current.GetInstance<IMvxAndroidCurrentTopActivity>(); }
        }
        public TextViewGroupInfoBinding(TextView androidControl) : base(androidControl)
        {
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        }

        protected override void SetValueToView(TextView control, GroupState value)
        {
            if (value == null)
            {
                control.TextFormatted = new SpannableString(string.Empty);
                return;
            }

            string fullInfo = GetGroupInformationText(value);
            string errorInfo = GetInformationByInvalidAnswers(value);

            var mainColor = GetGroupColorByStatus(value);
            var errorColor = this.GetColorFromResources(Resource.Color.group_with_invalid_answers);

            var spannableString = new SpannableString(fullInfo);

            spannableString.SetSpan(new ForegroundColorSpan(mainColor), 0, spannableString.Length(), SpanTypes.ExclusiveExclusive);

            if (fullInfo.Contains(errorInfo))
            {
                spannableString.SetSpan(new ForegroundColorSpan(errorColor), fullInfo.IndexOf(errorInfo), fullInfo.IndexOf(errorInfo) + errorInfo.Length, SpanTypes.ExclusiveExclusive);
            }

            control.TextFormatted = spannableString;
        }

        private static string GetGroupInformationText(GroupState groupState)
        {
            switch (groupState.Status)
            {
                case GroupStatus.NotStarted:
                    return UIResources.Interview_Group_Status_NotStarted;

                case GroupStatus.Started:
                    return string.Format(UIResources.Interview_Group_Status_StartedIncompleteFormat, GetInformationByQuestionsAndAnswers(groupState));

                case GroupStatus.Completed:
                    return string.Format(UIResources.Interview_Group_Status_CompletedFormat, GetInformationByQuestionsAndAnswers(groupState));

                case GroupStatus.StartedInvalid:
                    return string.Format(UIResources.Interview_Group_Status_StartedIncompleteFormat,
                        string.Format("{0}, {1}", GetInformationByQuestionsAndAnswers(groupState), GetInformationByInvalidAnswers(groupState)));

                case GroupStatus.CompletedInvalid:
                    return string.Format(UIResources.Interview_Group_Status_CompletedFormat,
                        string.Format("{0}, {1}", GetInformationByQuestionsAndAnswers(groupState), GetInformationByInvalidAnswers(groupState)));

                default:
                    return GetInformationByQuestionsAndAnswers(groupState);
            }
        }

        private Color GetGroupColorByStatus(GroupState groupState)
        {
            int groupTitleTextColorId;
            switch (groupState.Status)
            {
                case GroupStatus.NotStarted:
                    groupTitleTextColorId = Resource.Color.group_not_started;
                    break;
                case GroupStatus.Completed:
                case GroupStatus.CompletedInvalid:
                    groupTitleTextColorId = Resource.Color.group_completed;
                    break;
                default:
                    groupTitleTextColorId = Resource.Color.group_started;
                    break;
            }

            return this.GetColorFromResources(groupTitleTextColorId);
        }

        private Color GetColorFromResources(int resourceId)
        {
            return this.CurrentTopActivity.Activity.Resources.GetColor(resourceId);
        }

        private static string GetInformationByQuestionsAndAnswers(GroupState value)
        {
            var subGroupsText = GetInformationBySubgroups(value);

            var details = string.Format("{0}, {1}",
                value.AnsweredQuestionsCount == 1
                    ? UIResources.Interview_Group_AnsweredQuestions_One
                    : string.Format(UIResources.Interview_Group_AnsweredQuestions_Many, value.AnsweredQuestionsCount),
                subGroupsText);

            return details;
        }

        private static string GetInformationByInvalidAnswers(GroupState value)
        {
            return value.InvalidAnswersCount == 1
                ? UIResources.Interview_Group_InvalidAnswers_One
                : string.Format(UIResources.Interview_Group_InvalidAnswers_ManyFormat, value.InvalidAnswersCount);
        }

        private static string GetInformationBySubgroups(GroupState value)
        {
            switch (value.SubgroupsCount)
            {
                case 0:
                    return UIResources.Interview_Group_Subgroups_Zero;

                case 1:
                    return UIResources.Interview_Group_Subgroups_One;

                default:
                    return string.Format(UIResources.Interview_Group_Subgroups_ManyFormat, value.SubgroupsCount);
            }
        }
    }
}