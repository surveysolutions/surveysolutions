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
using WB.Core.SharedKernels.SurveyManagement.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Merger
{
    internal class when_merging_questionnaire_and_interview_with_question_linked_to_roster_and_answered_only_roster_size_question : InterviewDataAndQuestionnaireMergerTestContext
    {
        Establish context = () =>
        {
            rosterSizeQuestionId = Guid.Parse("10000000000000000000000000000000");
            linkedOnNestedRosterQuestionId = Guid.Parse("33333333333333333333333333333333");

            rosterId = Guid.Parse("44444444444444444444444444444444");
            rosterTitleQuestionId = Guid.Parse("20000000000000000000000000000000");

            interviewId = Guid.Parse("43333333333333333333333333333333");

            questionnaire =
                Create.QuestionnaireDocumentWithOneChapter(children:
                    new IComposite[]
                    {
                        Create.NumericIntegerQuestion(id: rosterSizeQuestionId, variable: "roster_size", linkedToRosterId: rosterId),
                        Create.Roster(rosterId: rosterId, variable: "ros", 
                            children: new[]
                            {
                                Create.TextListQuestion(questionId: rosterTitleQuestionId, variable: "roster_title")
                            },
                            rosterSizeSourceType: RosterSizeSourceType.Question, 
                            rosterSizeQuestionId: rosterSizeQuestionId,
                            rosterTitleQuestionId: rosterTitleQuestionId)
                    });

            interview = CreateInterviewData(interviewId);

            interview.Levels["#"].QuestionsSearchCache.Add(rosterSizeQuestionId, Create.InterviewQuestion(rosterSizeQuestionId, 2));

            AddInterviewLevel(interview, new ValueVector<Guid> { rosterSizeQuestionId }, Create.RosterVector(0).ToArray());
            AddInterviewLevel(interview, new ValueVector<Guid> { rosterSizeQuestionId }, Create.RosterVector(1).ToArray());

            user = Mock.Of<UserDocument>();
            merger = CreateMerger(questionnaire);
        };

        Because of = () =>
            mergeResult = merger.Merge(interview, questionnaire, user.GetUseLight(), null);


        It should_question_has_2_options = () =>
            GetQuestion(mergeResult, rosterSizeQuestionId, new decimal[0]).Options.Count.ShouldEqual(2);

        It should_question_has_empty_values_in_options = () =>
            GetQuestion(mergeResult, rosterSizeQuestionId, new decimal[0]).Options.Select(x => x.Label).ShouldContainOnly("", "");


        private static InterviewDataAndQuestionnaireMerger merger;
        private static InterviewDetailsView mergeResult;
        private static InterviewData interview;
        private static QuestionnaireDocument questionnaire;
        private static UserDocument user;
        
        private static Guid rosterSizeQuestionId;
        private static Guid linkedOnNestedRosterQuestionId;
        private static Guid rosterId;
        private static Guid rosterTitleQuestionId;
        private static Guid interviewId;
    }
}