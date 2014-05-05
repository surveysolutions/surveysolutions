﻿using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.Merger
{
    internal class when_merging_questionnaire_and_interview_data_with_group_and_fixed_roster_with_titles : InterviewDataAndQuestionnaireMergerTestContext
    {
        Establish context = () =>
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
            merger = CreateMerger();
            var questionnaireDocument = CreateQuestionnaireDocumentWithGroupAndFixedRoster(groupId, groupTitle, fixedRosterId, fixedRosterTitle, rosterFixedTitles);
            interview = CreateInterviewDataForQuestionnaireWithGroupAndFixedRoster(interviewId, groupId, groupTitle, fixedRosterId, fixedRosterTitle, rosterFixedTitles);
            questionnaire = CreateQuestionnaireWithVersion(questionnaireDocument);
            questionnaireReferenceInfo = CreateQuestionnaireReferenceInfo();
            questionnaireRosters = CreateQuestionnaireRosterStructureWithOneFixedRoster(fixedRosterId);
            user = Mock.Of<UserDocument>();
        };

        Because of = () =>
            mergeResult = merger.Merge(interview, questionnaire, questionnaireReferenceInfo, questionnaireRosters, user);

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
        private static QuestionnaireDocumentVersioned questionnaire;
        private static ReferenceInfoForLinkedQuestions questionnaireReferenceInfo;
        private static QuestionnaireRosterStructure questionnaireRosters;
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
