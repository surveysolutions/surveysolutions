using System;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateFilteredComboboxOptionsHandlerTests
{
    internal class when_updating_filtered_combobox_options_and_question_is_not_a_filtered_combobox : QuestionnaireTestsContext
    {
        [NUnit.Framework.Test] public void should_throw_QuestionnaireException () {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddSingleOptionQuestion(
                questionId,
                chapterId,
                title: "text",
                variableName: "var",
                responsibleId: responsibleId
            );

            exception = Assert.Throws<QuestionnaireException>(() =>
                questionnaire.UpdateFilteredComboboxOptions(questionId: questionId, responsibleId: responsibleId, options: options));

            exception.Message.ToLower().ToSeparateWords().Should().Contain(new[] { "not", "combo", "box" });
        }

        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static QuestionnaireCategoricalOption[] options = new [] { Create.QuestionnaireCategoricalOption(1, "Option 1"), Create.QuestionnaireCategoricalOption(2, "Option 2") };
    }
}
