using System;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateTextQuestionHandlerTests
{
    internal class when_updating_text_question_and_title_contains_substitution_to_self : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddQRBarcodeQuestion(questionId,
                    chapterId,
                        responsibleId,
                        title: "old title",
                        variableName: "old_variable_name",
                        instructions: "old instructions",
                        enablementCondition: "old condition");
            BecauseOf();
        }

        private void BecauseOf() =>
                 questionnaire.UpdateTextQuestion(
                     new UpdateTextQuestion(
                         questionnaire.Id,
                         questionId,
                         responsibleId,
                         new CommonQuestionParameters() { Title = titleWithSubstitutionToSelf, VariableName = variableName },
                         null, scope, false,
                         new System.Collections.Generic.List<ValidationCondition>()));

        [NUnit.Framework.Test] public void should_update_question_text () =>
            questionnaire.QuestionnaireDocument.GetQuestion<TextQuestion>(questionId)
                .QuestionText.Should().Be(titleWithSubstitutionToSelf);

        private static Questionnaire questionnaire;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private const string variableName = "var";
        private static string titleWithSubstitutionToSelf = string.Format("title with substitution to self - %{0}%", variableName);
        
        private static QuestionScope scope = QuestionScope.Interviewer;
    }
}