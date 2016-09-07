using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.AddTextQuestionHandlerTests
{
    internal class when_adding_text_question_with_substitution_referencing_valid_variable_but_another_question_has_null_variable_name : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.AddQuestion(Create.Event.NewQuestionAdded( publicKey : Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"), stataExportCaption : "valid_varname", groupPublicKey : chapterId ));
            questionnaire.AddQuestion(Create.Event.NewQuestionAdded( publicKey : Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE"), stataExportCaption : null, groupPublicKey : chapterId ));
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.AddTextQuestion(
                    questionId: questionId,
                    parentGroupId: chapterId,
                    title: "title %valid_varname%",
                    variableName: "varname",
                    variableLabel: null,
                    isPreFilled: false,
                    scope: QuestionScope.Interviewer,
                    enablementCondition: string.Empty,
                    validationExpression: string.Empty,
                    validationMessage: string.Empty,
                    instructions: string.Empty,
                    mask: null,
                    responsibleId: responsibleId));

        It should_not_fail = () =>
            exception.ShouldBeNull();

        private static Exception exception;
        private static Questionnaire questionnaire;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("22220000000000000000000000000000");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}