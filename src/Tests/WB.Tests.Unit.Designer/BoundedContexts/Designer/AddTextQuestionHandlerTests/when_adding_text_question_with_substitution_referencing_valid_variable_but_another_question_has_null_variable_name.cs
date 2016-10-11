using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.AddTextQuestionHandlerTests
{
    internal class when_adding_text_question_with_substitution_referencing_valid_variable_but_another_question_has_null_variable_name : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);

            questionnaire.AddNumericQuestion(Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"), chapterId ,responsibleId, variableName : "valid_varname");
            questionnaire.AddDefaultTypeQuestionAdnMoveIfNeeded(new AddDefaultTypeQuestion(questionnaire.Id, Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE"), chapterId, "title" , responsibleId ));
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.AddTextQuestion(
                    questionId: questionId,
                    parentId: chapterId,
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