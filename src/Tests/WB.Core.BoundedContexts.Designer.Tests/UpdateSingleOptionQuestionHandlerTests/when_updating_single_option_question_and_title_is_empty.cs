using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests;

namespace WB.Core.BoundedContexts.Designer.Tests.UpdateSingleOptionQuestionHandlerTests
{
    internal class when_updating_single_option_question_and_title_is_empty : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new QRBarcodeQuestionAdded()
            {
                QuestionId = questionId,
                ParentGroupId = chapterId,
                Title = "old title",
                VariableName = "old_variable_name",
                IsMandatory = false,
                Instructions = "old instructions",
                EnablementCondition = "old condition",
                ResponsibleId = responsibleId
            });
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.UpdateSingleOptionQuestion(
                    questionId: questionId,
                    title: emptyTitle,
                    variableName: variableName,
                    variableLabel: null,
                    isMandatory: isMandatory,
                    isPreFilled: isPreFilled,
                    scope: scope,
                    enablementCondition: enablementCondition,
                    validationExpression: validationExpression,
                    validationMessage: validationMessage,
                    instructions: instructions,
                    responsibleId: responsibleId,
                    options:options,
                    linkedToQuestionId: linkedToQuestionId,
                    isFilteredCombobox: isFilteredCombobox,
                    isCascadingCombobox: isCascadingCombobox
                    ));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__question__exist__ = () =>
            new[] { "empty", "title" }.ShouldEachConformTo(
                keyword => exception.Message.ToLower().Contains(keyword));


        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string variableName = "qr_barcode_question";
        private static bool isMandatory = true;
        private static string emptyTitle = "";
        private static string instructions = "intructions";
        private static bool isPreFilled = false;
        private static QuestionScope scope = QuestionScope.Interviewer;
        private static string enablementCondition = null;
        private static string validationExpression = null;
        private static string validationMessage = "";
        private static Option[] options = new Option[] { new Option(Guid.NewGuid(), "1", "Option 1"), new Option(Guid.NewGuid(), "2", "Option 2"), };
        private static Guid? linkedToQuestionId = (Guid?)null;
        private static bool isFilteredCombobox = false;
        private static bool isCascadingCombobox = false;
    }
}