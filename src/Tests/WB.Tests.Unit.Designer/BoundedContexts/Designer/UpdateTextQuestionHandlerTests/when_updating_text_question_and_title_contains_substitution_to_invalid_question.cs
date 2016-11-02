using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Exceptions;

using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateTextQuestionHandlerTests
{
    internal class when_updating_text_question_and_title_contains_substitution_to_invalid_question : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddQRBarcodeQuestion(
                questionId,
                chapterId,
                responsibleId,
                title: "old title",
                variableName: "old_variable_name");
        };

        Because of = () =>
             exception = Catch.Exception(() =>
                 questionnaire.UpdateTextQuestion(
                     new UpdateTextQuestion(
                         questionnaire.Id,
                         questionId,
                         responsibleId,
                         new CommonQuestionParameters() { Title = titleWithSubstitution, VariableName = variableName },
                         null, scope, false,
                         new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>())));
        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__title___not_existing_variable_name__as__substitution__ = () =>
            new[] { "text", "contains", "unknown", "substitution" }.ShouldEachConformTo(
                keyword => exception.Message.ToLower().Contains(keyword));


        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private const string substitutionVariableName = "notExistingVar";
        private static string titleWithSubstitution = string.Format("title with substitution - %{0}%", substitutionVariableName);
        private static string variableName = "qr_barcode_question";
        
        private static QuestionScope scope = QuestionScope.Interviewer;
        
    }
}