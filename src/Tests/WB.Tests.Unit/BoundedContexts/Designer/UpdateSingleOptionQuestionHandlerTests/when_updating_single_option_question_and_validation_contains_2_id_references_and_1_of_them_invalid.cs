using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Moq;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.UpdateSingleOptionQuestionHandlerTests
{
    [Ignore("reference validation is turned off")]
    internal class when_updating_single_option_question_and_validation_contains_2_id_references_and_1_of_them_invalid : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.GetIdentifiersUsedInExpression(validationExpression) == new[] { existingQuestionId.ToString(), notExistingQuestionId.ToString() });

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId, expressionProcessor: expressionProcessor);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(Create.Event.NumericQuestionAdded(publicKey : existingQuestionId, groupPublicKey : chapterId ));
            questionnaire.Apply(Create.Event.NewQuestionAdded(
publicKey: questionId,
groupPublicKey: chapterId,
questionText: "old title",
stataExportCaption: "old_variable_name",
instructions: "old instructions",
conditionExpression: "old condition",
responsibleId: responsibleId,
questionType: QuestionType.QRBarcode
));
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.UpdateSingleOptionQuestion(
                    questionId: questionId,
                    title: title,
                    variableName: variableName,
                    variableLabel: null,
                    isPreFilled: isPreFilled,
                    scope: scope,
                    enablementCondition: enablementCondition,
                    hideIfDisabled: false,
                    instructions: instructions,
                    responsibleId: responsibleId,
                    options: options,
                    linkedToEntityId: linkedToQuestionId,
                    isFilteredCombobox: isFilteredCombobox,
                    cascadeFromQuestionId: �ascadeFromQuestionId, validationConditions: new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>()));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__not__valid__expression__ = () =>
            new[] { "not", "valid", "expression" }.ShouldEachConformTo(
                keyword => exception.Message.ToLower().Contains(keyword));


        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid existingQuestionId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid notExistingQuestionId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string variableName = "qr_barcode_question";
        private static string title = "title";
        private static string instructions = "intructions";
        private static bool isPreFilled = false;
        private static QuestionScope scope = QuestionScope.Interviewer;
        private static string enablementCondition = null;
        private static string validationExpression = string.Format("[{0}] > 1 and [{1}] < 2", existingQuestionId, notExistingQuestionId);
        private static Option[] options = new Option[] { new Option(Guid.NewGuid(), "1", "Option 1"), new Option(Guid.NewGuid(), "2", "Option 2"), };
        private static Guid? linkedToQuestionId = (Guid?)null;
        private static bool isFilteredCombobox = false;
        private static Guid? �ascadeFromQuestionId = (Guid?)null;
    }
}