using System;
using System.Globalization;
using Cirrious.CrossCore.Converters;
using WB.Core.BoundedContexts.QuestionnaireTester.Properties;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;

namespace WB.UI.QuestionnaireTester.Converters
{
    public class GetGroupInfoConverter : MvxValueConverter<GroupViewModel, string>
    {
        protected override string Convert(GroupViewModel value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.AnsweredQuestionsCount == 0)
                return UIResources.Interview_Group_NotStarted;

            string subGroupsText = value.SubgroupsCount == 0
                ? UIResources.Interview_Group_NoSubgroups
                : string.Format("{0} {1}", value.SubgroupsCount,
                    value.SubgroupsCount == 1
                        ? UIResources.Interview_Group_OneSubgroup
                        : UIResources.Interview_Group_ManySubgroups);


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
    }
}