using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Widget;
using Cirrious.CrossCore.Droid.Platform;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.QuestionnaireTester.Properties;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;

namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class TextViewGroupInfoBinding : BindingWrapper<TextView, GroupViewModel>
    {
        private IMvxAndroidCurrentTopActivity CurrentTopActivity
        {
            get { return ServiceLocator.Current.GetInstance<IMvxAndroidCurrentTopActivity>(); }
        }
        public TextViewGroupInfoBinding(TextView androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(TextView androidControl, GroupViewModel value)
        {
            if (value == null) return;

            string groupInformationText;

            if (value.Status == GroupStatus.NotStarted)
            {
                groupInformationText = UIResources.Interview_Group_NotStarted;
            }
            else if (value.Status == GroupStatus.StartedInvalid || value.Status == GroupStatus.CompletedInvalid)
            {
                groupInformationText = string.Format("{0}, {1}", GetInformationByQuestionsAndAnswers(value),
                    GetInformationByInvalidAnswers(value));
            }
            else
            {
                groupInformationText = GetInformationByQuestionsAndAnswers(value);
            }

            var spannableText = new SpannableString(groupInformationText);

            var groupColorByStatus = GetGroupColorByStatus(value);
            spannableText.SetSpan(new ForegroundColorSpan(groupColorByStatus), 0, spannableText.Length(), SpanTypes.ExclusiveExclusive);

            if (value.Status == GroupStatus.StartedInvalid || value.Status == GroupStatus.CompletedInvalid)
            {
                var groupWithInvalidAnswersColor = this.GetColorFromResources(Resource.Color.group_with_invalid_answers);

                spannableText.SetSpan(new ForegroundColorSpan(groupWithInvalidAnswersColor),
                    GetInformationByQuestionsAndAnswers(value).Length + 1, groupInformationText.Length, SpanTypes.ExclusiveExclusive);
            }
                

            androidControl.TextFormatted = spannableText;
        }

        private Color GetGroupColorByStatus(GroupViewModel value)
        {
            int groupTitleTextColorId;
            switch (value.Status)
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

        private static string GetInformationByInvalidAnswers(GroupViewModel value)
        {
            return string.Format("{0} {1}", value.InvalidAnswersCount,
                value.InvalidAnswersCount == 1
                    ? UIResources.Interview_Group_OneInvalidAnswer
                    : UIResources.Interview_Group_ManyInvalidAnswers);
        }

        private static string GetInformationByQuestionsAndAnswers(GroupViewModel value)
        {
            var subGroupsText = GetInformationBySubgroups(value);

            if (value.QuestionsCount == value.AnsweredQuestionsCount)
                return string.Format(UIResources.Interview_Group_CompletedFormat,
                    value.QuestionsCount,
                    value.QuestionsCount == 1
                        ? UIResources.Interview_Group_OneQuestion_Answered
                        : UIResources.Interview_Group_ManyQuestions_Answered,
                    subGroupsText);

            return string.Format(UIResources.Interview_Group_StartedFormat,
                value.AnsweredQuestionsCount,
                value.AnsweredQuestionsCount == 1
                    ? UIResources.Interview_Group_OneQuestion_Answered
                    : UIResources.Interview_Group_ManyQuestions_Answered,
                subGroupsText);

        }

        private static string GetInformationBySubgroups(GroupViewModel value)
        {
            return value.SubgroupsCount == 0
                ? UIResources.Interview_Group_NoSubgroups
                : string.Format("{0} {1}", value.SubgroupsCount,
                    value.SubgroupsCount == 1
                        ? UIResources.Interview_Group_OneSubgroup
                        : UIResources.Interview_Group_ManySubgroups);
        }
    }
}