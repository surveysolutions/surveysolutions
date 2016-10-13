using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateFilteredComboboxOptionsHandlerTests
{
    internal class when_updating_filtered_combobox_options_and_1_option_has_17_digits : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddSingleOptionQuestion(
                questionId,
                chapterId,
                responsibleId,
                title: "text",
                variableName: "var",
                isFilteredCombobox: true
            );
        };

        Because of = () =>
            questionnaireException = Catch.Only<QuestionnaireException>(() =>
                questionnaire.UpdateFilteredComboboxOptions(questionId: questionId, responsibleId: responsibleId, options: options));

        It should_throw_QuestionnaireException = () =>
            questionnaireException.ShouldNotBeNull();

        It should_throw_exception_with_message_containting__values____too_long__and_value_itself = () =>
            new[] { "values", "too long", "12345678901234567" }.ShouldEachConformTo(
                keyword => questionnaireException.Message.ToLower().Contains(keyword));

        private static Questionnaire questionnaire;
        private static QuestionnaireException questionnaireException;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Option[] options = { new Option(Guid.NewGuid(), "12345678901234567", "Option 1"), new Option(Guid.NewGuid(), "2", "Option 2") };
    }
}