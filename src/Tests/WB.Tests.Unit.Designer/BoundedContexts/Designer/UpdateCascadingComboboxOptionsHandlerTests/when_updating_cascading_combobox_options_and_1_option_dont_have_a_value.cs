using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateCascadingComboboxOptionsHandlerTests
{
    internal class when_updating_cascading_combobox_options_and_1_option_dont_have_a_value : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });

            questionnaire.AddQuestion(Create.Event.NewQuestionAdded(
                publicKey: parentQuestionId,
                groupPublicKey: chapterId,
                questionType: QuestionType.SingleOption,
                questionText: "text",
                stataExportCaption: "var",

                responsibleId: responsibleId,
                answers: new Answer[]
                {
                    new Answer { AnswerText = "Option 1", AnswerValue = "1" },
                    new Answer { AnswerText = "Option 2", AnswerValue = "2" }
                }
            ));

            questionnaire.AddQuestion(Create.Event.NewQuestionAdded(
                publicKey: questionId,
                groupPublicKey: chapterId,
                questionType: QuestionType.SingleOption,
                questionText: "text",
                stataExportCaption: "var2",
                isFilteredCombobox: false,
                responsibleId: responsibleId,
                cascadeFromQuestionId: parentQuestionId
            ));
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.UpdateCascadingComboboxOptions(questionId: questionId, responsibleId: responsibleId, options: options));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__value_required__ = () =>
             new[] { "value", "required" }.ShouldEachConformTo(
                    keyword => exception.Message.ToLower().Contains(keyword));


        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid parentQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Option[] options = new[] { new Option(Guid.NewGuid(), "1", "Option 1"), new Option(Guid.NewGuid(), string.Empty, "Option 2") };
    }
}