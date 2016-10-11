using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateFilteredComboboxOptionsHandlerTests
{
    internal class when_updating_filtered_combobox_options_and_options_count_less_than_2 : QuestionnaireTestsContext
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
            exception = Catch.Exception(() =>
                questionnaire.UpdateFilteredComboboxOptions(questionId: questionId, responsibleId: responsibleId, options: options));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__should_have_two_options_at_least__ = () =>
             new[] { "should have", "two options at least" }.ShouldEachConformTo(
                    keyword => exception.Message.ToLower().Contains(keyword));


        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Option[] options = new[] { new Option(Guid.NewGuid(), "1", "Option 1") };
    }
}