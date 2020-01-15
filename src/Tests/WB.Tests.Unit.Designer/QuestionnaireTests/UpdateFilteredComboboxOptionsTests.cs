using System;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using NUnit.Framework;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.QuestionnaireTests
{
    internal class UpdateFilteredComboboxOptionsTests : QuestionnaireTestsContext
    {
        [Test]
        public void when_updating_multi_filtered_combobox_options_and_question_had_saved_maxAllowedAnswers_count_then_maxAllowedAnswers_counter_should_not_be_changed()
        {
            Guid questionId = Guid.Parse("11111111111111111111111111111111");
            Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

            QuestionnaireCategoricalOption[] options = new[]
            {
                Create.QuestionnaireCategoricalOption(1, "Option 1"),
                Create.QuestionnaireCategoricalOption(2, "Option 2")
            };
            var maxAllowedAnswers = 2;

            var questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
                questionnaire.AddGroup(chapterId, responsibleId: responsibleId);
                questionnaire.AddMultiOptionQuestion(
                    questionId,
                    chapterId,
                    title: "text",
                    variableName: "var",
                    responsibleId: responsibleId,
                    isFilteredCombobox: true,
                    maxAllowedAnswers: maxAllowedAnswers
                );

                questionnaire.UpdateFilteredComboboxOptions(questionId: questionId, responsibleId: responsibleId, options: options);

                Assert.That(maxAllowedAnswers, Is.EqualTo(questionnaire.QuestionnaireDocument
                    .FirstOrDefault<MultyOptionsQuestion>(x => x.PublicKey == questionId).MaxAllowedAnswers));
        }
    }
}
