using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests;

namespace WB.Tests.Unit.BoundedContexts.Designer.AddTextQuestionHandlerTests
{
    [Ignore("reference validation is turned off")]
    internal class when_adding_text_question_and_enablementCondition_reference_not_existing_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(Create.Event.AddGroup(chapterId));
            questionnaire.Apply(Create.Event.AddGroup(rosterId, parentId: chapterId, variableName: "roster"));
            questionnaire.Apply(Create.Event.GroupBecameRoster(rosterId));
            questionnaire.Apply(Create.Event.RosterChanged(rosterId,  rosterType: RosterSizeSourceType.FixedTitles, titles: new[] { new Tuple<decimal, string>(1,"1"), new Tuple<decimal, string>(2,"2") }));
            questionnaire.Apply(Create.Event.AddTextQuestion(rosterQuestionId, parentId: rosterId));
            questionnaire.Apply(Create.Event.UpdateNumericIntegerQuestion(rosterQuestionId, variableName: "age"));
            questionnaire.Apply(Create.Event.AddTextQuestion(existingQuestionId, parentId: chapterId));
            questionnaire.Apply(Create.Event.UpdateNumericIntegerQuestion(existingQuestionId, variableName: "maxAge"));

            RegisterExpressionProcessorMock("absentRoster.Max(x => x.age) > maxAge", new[] { "absentRoster", "age", "maxAge" });
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.AddTextQuestion(
                    questionId: questionId,
                    parentGroupId: chapterId,
                    title: "title",
                    variableName: "text_question",
                    variableLabel: null,
                    isMandatory: false,
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