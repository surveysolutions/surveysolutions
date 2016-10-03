using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Views;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Merger
{
    internal class when_merging_questionnaire_and_interview_data_with_group_and_fixed_roster_with_titles : InterviewDataAndQuestionnaireMergerTestContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithGroupAndFixedRoster(groupId, groupTitle, fixedRosterId, fixedRosterTitle, rosterFixedTitles);
            interview = CreateInterviewDataForQuestionnaireWithGroupAndFixedRoster(interviewId, groupId, groupTitle, fixedRosterId, fixedRosterTitle, rosterFixedTitles);
            
            user = Mock.Of<UserDocument>();
            merger = CreateMerger(questionnaire);
        };

        Because of = () =>
            mergeResult = merger.Merge(interview, questionnaire, user.GetUseLight(), null, null);

        It should_create_3_group_screens_plus_questionnaire_screen_ = () =>
            mergeResult.Groups.Count.ShouldEqual(3+1);

        It should_set__groupTitle__to_regular_group_with_groupId_id = () =>
            mergeResult.Groups.Single(x => x.Id == groupId).Title.ShouldEqual(groupTitle);

        It should_set_first_fixed_roster_title_ = () =>
            mergeResult.Groups.Single(x => x.Id == fixedRosterId && x.RosterVector.SequenceEqual(new decimal[] { 0 }))
                .Title.ShouldEqual(string.Format(fixedRosterTitleFormat, rosterFixedTitles[0]));

        It should_set_second_fixed_roster_title_ = () =>
            mergeResult.Groups.Single(x => x.Id == fixedRosterId && x.RosterVector.SequenceEqual(new decimal[] { 1 }))
                .Title.ShouldEqual(string.Format(fixedRosterTitleFormat, rosterFixedTitles[1]));


        private static InterviewDataAndQuestionnaireMerger merger;
        private static InterviewDetailsView mergeResult;
        private static InterviewData interview;
        private static QuestionnaireDocument questionnaire;
        private static UserDocument user;
        private static Guid groupId = Guid.Parse("11111111111111111111111111111111");
        private static Guid fixedRosterId = Guid.Parse("22222222222222222222222222222222");
        private static Guid interviewId = Guid.Parse("33333333333333333333333333333333");
        private static string fixedRosterTitle = "Fixed roster";
        private static string fixedRosterTitleFormat = string.Format("{0}: {{0}}", fixedRosterTitle);
        private static string[] rosterFixedTitles = { "Title1", "Title2" };
        private static string groupTitle = "Group Title";
    }
}
