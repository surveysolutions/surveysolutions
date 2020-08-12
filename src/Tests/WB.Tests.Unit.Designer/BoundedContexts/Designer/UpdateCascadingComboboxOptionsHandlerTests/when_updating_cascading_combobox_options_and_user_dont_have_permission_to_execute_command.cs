using System;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateCascadingComboboxOptionsHandlerTests
{
    internal class when_updating_cascading_combobox_options_and_user_dont_have_permission_to_execute_command : QuestionnaireTestsContext
    {
        [NUnit.Framework.Test] public void should_throw_QuestionnaireException() {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddSingleOptionQuestion(
                parentQuestionId,
                chapterId,
                title: "text",
                variableName: "var",
                responsibleId: responsibleId,
                options: new Option[]
                {
                    new Option(title : "Option 1", value : "1"),
                    new Option(title : "Option 2", value : "2")
                }
            );
            questionnaire.AddSingleOptionQuestion(
                questionId,
                chapterId,
                title: "text",
                variableName : "q1",
                isFilteredCombobox : false,
                responsibleId : responsibleId,
                cascadeFromQuestionId : parentQuestionId
           );

            exception = Assert.Throws<QuestionnaireException>(() =>
                questionnaire.UpdateCascadingComboboxOptions(questionId: questionId, responsibleId: notExistingResponsibleId, options: options));

            exception.Message.ToLower().ToSeparateWords().Should().Contain(new[] { "don't", "have", "permissions" });
        }

        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid parentQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid notExistingResponsibleId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static QuestionnaireCategoricalOption[] options = new[] { Create.QuestionnaireCategoricalOption(1, "Option 1"), Create.QuestionnaireCategoricalOption(2, "Option 2") };
    }
}
