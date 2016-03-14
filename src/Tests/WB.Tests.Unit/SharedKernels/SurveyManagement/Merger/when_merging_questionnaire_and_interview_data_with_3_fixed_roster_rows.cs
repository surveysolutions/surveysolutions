using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Merger
{
    internal class when_merging_questionnaire_and_interview_data_with_3_fixed_roster_rows : InterviewDataAndQuestionnaireMergerTestContext
    {
        Establish context = () =>
        {

            rosterId = Guid.Parse("20000000000000000000000000000000");
            var rosterSizeQuestionId = Guid.Parse("33333333333333333333333333333333");

            interviewId = Guid.Parse("43333333333333333333333333333333");

            questionnaire =
                Create.QuestionnaireDocument(children:
                    new []
                    {
                        Create.Roster(rosterId: rosterId, rosterSizeSourceType: RosterSizeSourceType.FixedTitles,
                            fixedTitles: new[] {"1", "2", "3"})
                    });

            interview = CreateInterviewData(interviewId);

            AddInterviewLevel(interview: interview, scopeVector: new ValueVector<Guid> { rosterId }, rosterVector: new decimal[] { 2 },
                answeredQuestions:new Dictionary<Guid, object>(),
                rosterTitles:new Dictionary<Guid, string>() { { rosterId, "3" } }, sortIndex:3);

            AddInterviewLevel(interview: interview, scopeVector: new ValueVector<Guid> { rosterId }, rosterVector: new decimal[] { 1 },
              answeredQuestions: new Dictionary<Guid, object>(),
              rosterTitles: new Dictionary<Guid, string>() { { rosterId, "2" } }, sortIndex: 2);

            AddInterviewLevel(interview: interview, scopeVector: new ValueVector<Guid> {rosterId},
                rosterVector: new decimal[] {0},
                answeredQuestions: new Dictionary<Guid, object>(),
                rosterTitles: new Dictionary<Guid, string>() {{rosterId, "1"}}, sortIndex: 1);
            
            user = Mock.Of<UserDocument>();
            merger = CreateMerger(questionnaire);
        };

        Because of = () =>
            mergeResult = merger.Merge(interview, questionnaire, user.GetUseLight(), null);

        It should_return_3_fixed_roster_rows = () =>
            mergeResult.Groups.Count(g => g.Id==rosterId).ShouldEqual(3);

        It should_second_group_be_roster_row_with_sort_index_1 = () =>
            mergeResult.Groups[1].RosterVector.ShouldEqual(new decimal[] { 0 });

        It should_third_group_be_roster_row_with_sort_index_2 = () =>
            mergeResult.Groups[2].RosterVector.ShouldEqual(new decimal[] { 1 });

        It should_fourth_group_be_roster_row_with_sort_index_3 = () =>
            mergeResult.Groups[3].RosterVector.ShouldEqual(new decimal[] { 2 });

        private static InterviewDataAndQuestionnaireMerger merger;
        private static InterviewDetailsView mergeResult;
        private static InterviewData interview;
        private static QuestionnaireDocument questionnaire;
        private static UserDocument user;

        private static Guid rosterId;
        private static Guid interviewId;
    }
}