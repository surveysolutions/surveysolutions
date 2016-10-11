using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.AddTextQuestionHandlerTests
{
    [Ignore("reference validation is turned off")]
    internal class when_adding_text_question_and_enablementCondition_reference_not_existing_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.GetIdentifiersUsedInExpression("absentRoster.Max(x => x.age) > maxAge") == new[] { "absentRoster", "age", "maxAge" });

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId, expressionProcessor: expressionProcessor);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddGroup(rosterId, parentGroupId: chapterId, variableName: "roster");
            questionnaire.MarkGroupAsRoster(Create.Event.GroupBecameRoster(rosterId));
            questionnaire.ChangeRoster(Create.Event.RosterChanged(rosterId,  rosterType: RosterSizeSourceType.FixedTitles,
                titles: new[] { new FixedRosterTitle(1, "1"), new FixedRosterTitle(2, "2") }));

            questionnaire.AddNumericQuestion(rosterQuestionId, parentId: rosterId,responsibleId:responsibleId, variableName:"age");
            
            questionnaire.AddNumericQuestion(existingQuestionId, parentId: chapterId, responsibleId:responsibleId, variableName: "maxAge");
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.AddTextQuestion(
                    questionId: questionId,
                    parentId: chapterId,
                    title: "title",
                    variableName: "text_question",
                    variableLabel: null,
                    isPreFilled: false,
                    scope: QuestionScope.Interviewer,
                    enablementCondition: "absentRoster.Max(x => x.age) > maxAge",
                    validationExpression: "",
                    validationMessage: "",
                    instructions: "intructions",
                    mask: null,
                    responsibleId: responsibleId));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__not__valid__expression__ = () =>
            new[] { "not", "valid", "expression", "absentroster", "question or roster" }.ShouldEachConformTo(
                keyword => exception.Message.ToLower().Contains(keyword));

        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid existingQuestionId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid rosterQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid notExistingQuestionId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid rosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}