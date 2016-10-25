using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateTextQuestionHandlerTests
{
    internal class when_updating_text_question_and_title_contains_substitution_to_variable : QuestionnaireTestsContext
    {
        private Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddQRBarcodeQuestion(
                questionId,
                chapterId,
                responsibleId,
                title: "title",
                variableName: "q1",
                instructions: "instructions",
                enablementCondition: "condition");

            questionnaire.AddVariable(
                Guid.NewGuid(),
                responsibleId:responsibleId,
                parentId: chapterId,
                variableType: VariableType.String,
                variableName: variableName,
                variableExpression: "text + text");
        };

        Because of = () =>
             exception = Catch.Exception(() =>
                 questionnaire.UpdateTextQuestion(
                     new UpdateTextQuestion(
                         questionnaire.Id,
                         questionId,
                         responsibleId,
                         new CommonQuestionParameters() { Title = titleWithSubstitutionToVariable, VariableName = "q1" },
                         null, scope, false,
                         new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>())));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeNull();


        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private const string variableName = "var";
        private static string titleWithSubstitutionToVariable = $"title with substitution to variable - %{variableName}%";
        
        private static QuestionScope scope = QuestionScope.Interviewer;
        
    }
}