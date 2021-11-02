using System;
using System.Collections.Generic;
using FluentAssertions;
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
        [NUnit.Framework.Test] public void should_not_throw_QuestionnaireException () {
            questionnaire = CreateQuestionnaire(responsibleId: ResponsibleId);
            questionnaire.AddGroup(ChapterId, responsibleId:ResponsibleId);
            questionnaire.AddQRBarcodeQuestion(
                QuestionId,
                ChapterId,
                ResponsibleId,
                title: "title",
                variableName: "q1",
                instructions: "instructions",
                enablementCondition: "condition");

            questionnaire.AddVariable(
                Guid.NewGuid(),
                responsibleId:ResponsibleId,
                parentId: ChapterId,
                variableType: VariableType.String,
                variableName: VariableName,
                variableExpression: "text + text");


            questionnaire.UpdateTextQuestion(
                new UpdateTextQuestion(
                    questionnaire.Id,
                    QuestionId,
                    ResponsibleId,
                    new CommonQuestionParameters() {Title = TitleWithSubstitutionToVariable, VariableName = "q1"},
                    null, scope, false,
                    new List<ValidationCondition>()));

        }

        private static Questionnaire questionnaire;
        private static readonly Guid QuestionId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid ChapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid ResponsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private const string VariableName = "var";
        private static readonly string TitleWithSubstitutionToVariable = $"title with substitution to variable - %{VariableName}%";
        
        private static QuestionScope scope = QuestionScope.Interviewer;
        
    }
}
