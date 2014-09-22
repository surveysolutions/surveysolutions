using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests;

namespace WB.Core.BoundedContexts.Designer.Tests.UpdateSingleOptionQuestionHandlerTests
{
    internal class when_updating_cascading_combobox_options_and_1_option_has_empty_parent_value : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewQuestionAdded
            {
                PublicKey = parentQuestionId,
                GroupPublicKey = chapterId,
                QuestionType = QuestionType.SingleOption,
                QuestionText = "text",
                StataExportCaption = "var",
                ResponsibleId = responsibleId,
                Answers = new Answer[]
                {
                    new Answer { AnswerText = "Option 1", AnswerValue = "1" },
                    new Answer { AnswerText = "Option 2", AnswerValue = "2" }
                }
            });
            questionnaire.Apply(new NewQuestionAdded
            {
                PublicKey = questionId,
                GroupPublicKey = chapterId,
                QuestionType = QuestionType.SingleOption,
                QuestionText = "text",
                StataExportCaption = "var2",
                ResponsibleId = responsibleId,
                CascadeFromQuestionId = parentQuestionId
            });
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.UpdateCascadingComboboxOptions(questionId: questionId, responsibleId: responsibleId, options: options));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__value_required__ = () =>
            new[] { "empty", "parentvalue" }.ShouldEachConformTo(
                keyword => exception.Message.ToLower().Contains(keyword));


        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid parentQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Option[] options =
        {
            new Option(Guid.NewGuid(), "1", "Option 1", "1"), 
            new Option(Guid.NewGuid(), "2", "Option 2", null)
        };
    }
}