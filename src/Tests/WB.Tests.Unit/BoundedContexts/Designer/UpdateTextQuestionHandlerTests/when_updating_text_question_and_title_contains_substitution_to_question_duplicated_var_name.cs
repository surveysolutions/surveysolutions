using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests;

namespace WB.Tests.Unit.BoundedContexts.Designer.UpdateTextQuestionHandlerTests
{
    internal class when_updating_text_question_and_title_contains_substitution_there_are_questions_duplicated_var_name : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(Create.Event.NumericQuestionAdded(
                publicKey: questionId,
                groupPublicKey: chapterId,
                questionText: "old title",
                stataExportCaption: "old_variable_name",
                instructions: "old instructions",
                conditionExpression: "old condition",
                responsibleId: responsibleId
            ));
            questionnaire.Apply(Create.Event.NumericQuestionAdded(
                publicKey: question1Id,
                groupPublicKey: chapterId,
                questionText: "old title",
                stataExportCaption: "duplicateVar",
                instructions: "old instructions",
                conditionExpression: "old condition",
                responsibleId: responsibleId
            ));
            questionnaire.Apply(Create.Event.NumericQuestionAdded(
                publicKey : question2Id,
                groupPublicKey : chapterId,
                questionText : "old title",
                stataExportCaption : "duplicateVar",
                instructions : "old instructions",
                conditionExpression: "old condition",
                responsibleId : responsibleId
            ));
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.UpdateTextQuestion(
                    questionId: questionId,
                    title: titleWithSubstitution,
                    variableName: variableName,
                    variableLabel: null,
                    isPreFilled: isPreFilled,
                    scope: scope,
                    enablementCondition: enablementCondition,
                    instructions: instructions,
                     mask: null,
                    responsibleId: responsibleId, validationCoditions: new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>()));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__title___not_existing_variable_name__as__substitution__ = () =>
            new[] { "text", "contains", "unknown", "substitution" }.ShouldEachConformTo(
                keyword => exception.Message.ToLower().Contains(keyword));


        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid question1Id = Guid.Parse("11111111111111111111111111111112");
        private static Guid question2Id = Guid.Parse("11111111111111111111111111111113");

        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private const string substitutionVariableName = "duplicateVar";
        private static string titleWithSubstitution = "title with substitution - %rostertitle%";
        private static string variableName = "qr_barcode_question";
        private static string instructions = "intructions";
        private static bool isPreFilled = false;
        private static QuestionScope scope = QuestionScope.Interviewer;
        private static string enablementCondition = null;
    }
}