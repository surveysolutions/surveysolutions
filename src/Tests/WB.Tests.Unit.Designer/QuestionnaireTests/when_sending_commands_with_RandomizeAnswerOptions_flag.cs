using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_sending_commands_with_RandomizeAnswerOptions_flag : QuestionnaireTestsContext
    {
        private static readonly Guid ResponsibleId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid ChapterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid SingleQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid MultiQuestionId = Guid.Parse("33333333333333333333333333333333");

        private Core.BoundedContexts.Designer.Aggregates.Questionnaire CreateQuestionnaire()
        {
            var questionnaire = CreateQuestionnaireWithOneGroup(ResponsibleId, groupId: ChapterId);
            questionnaire.AddSingleOptionQuestion(SingleQuestionId, ChapterId, ResponsibleId, variableName: "single");
            questionnaire.AddMultiOptionQuestion(MultiQuestionId, ChapterId, ResponsibleId, variableName: "multi");
            return questionnaire;
        }

        private static UpdateSingleOptionQuestion UpdateSingleOptionQuestion(Guid questionnaireId, bool isFilteredCombobox, bool randomizeAnswerOptions)
            => new UpdateSingleOptionQuestion(questionnaireId, SingleQuestionId, ResponsibleId,
                new CommonQuestionParameters { Title = "title", VariableName = "single" },
                validationExpression: null, validationMessage: null, scope: QuestionScope.Interviewer, isPreFilled: false,
                options: new[] { new Option(title: "one", value: "1"), new Option(title: "two", value: "2") },
                linkedToEntityId: null, isFilteredCombobox: isFilteredCombobox, cascadeFromQuestionId: null,
                validationConditions: new List<ValidationCondition>(), linkedFilterExpression: null,
                showAsList: null, showAsListThreshold: null, categoriesId: null)
            {
                RandomizeAnswerOptions = randomizeAnswerOptions
            };

        private static UpdateMultiOptionQuestion UpdateMultiOptionQuestion(bool isFilteredCombobox, bool yesNoView, bool randomizeAnswerOptions)
        {
            var command = Create.Command.UpdateMultiOptionQuestion(MultiQuestionId, ResponsibleId, "title", "multi",
                isFilteredCombobox: isFilteredCombobox, yesNoView: yesNoView);
            command.RandomizeAnswerOptions = randomizeAnswerOptions;
            return command;
        }

        [Test]
        public void should_be_false_by_default()
        {
            var questionnaire = CreateQuestionnaire();

            Assert.That(questionnaire.QuestionnaireDocument.Find<IQuestion>(SingleQuestionId).RandomizeAnswerOptions, Is.False);
            Assert.That(questionnaire.QuestionnaireDocument.Find<IQuestion>(MultiQuestionId).RandomizeAnswerOptions, Is.False);
        }

        [Test]
        public void should_set_flag_for_categorical_questions_displayed_as_list()
        {
            var questionnaire = CreateQuestionnaire();

            questionnaire.UpdateSingleOptionQuestion(UpdateSingleOptionQuestion(questionnaire.Id, isFilteredCombobox: false, randomizeAnswerOptions: true));
            questionnaire.UpdateMultiOptionQuestion(UpdateMultiOptionQuestion(isFilteredCombobox: false, yesNoView: false, randomizeAnswerOptions: true));

            Assert.That(questionnaire.QuestionnaireDocument.Find<IQuestion>(SingleQuestionId).RandomizeAnswerOptions, Is.True);
            Assert.That(questionnaire.QuestionnaireDocument.Find<IQuestion>(MultiQuestionId).RandomizeAnswerOptions, Is.True);
            Assert.That(questionnaire.QuestionnaireDocument.Find<IQuestion>(SingleQuestionId).RandomizeOrder, Is.False);
        }

        [Test]
        public void should_set_flag_for_yes_no_question()
        {
            var questionnaire = CreateQuestionnaire();

            questionnaire.UpdateMultiOptionQuestion(UpdateMultiOptionQuestion(isFilteredCombobox: false, yesNoView: true, randomizeAnswerOptions: true));

            Assert.That(questionnaire.QuestionnaireDocument.Find<IQuestion>(MultiQuestionId).RandomizeAnswerOptions, Is.True);
        }

        [Test]
        public void should_not_set_flag_for_combobox_questions()
        {
            var questionnaire = CreateQuestionnaire();

            questionnaire.UpdateSingleOptionQuestion(UpdateSingleOptionQuestion(questionnaire.Id, isFilteredCombobox: true, randomizeAnswerOptions: true));
            questionnaire.UpdateMultiOptionQuestion(UpdateMultiOptionQuestion(isFilteredCombobox: true, yesNoView: false, randomizeAnswerOptions: true));

            Assert.That(questionnaire.QuestionnaireDocument.Find<IQuestion>(SingleQuestionId).RandomizeAnswerOptions, Is.False);
            Assert.That(questionnaire.QuestionnaireDocument.Find<IQuestion>(MultiQuestionId).RandomizeAnswerOptions, Is.False);
        }

        [Test]
        public void should_reset_flag_when_question_is_switched_to_combobox()
        {
            var questionnaire = CreateQuestionnaire();
            questionnaire.UpdateSingleOptionQuestion(UpdateSingleOptionQuestion(questionnaire.Id, isFilteredCombobox: false, randomizeAnswerOptions: true));

            questionnaire.UpdateSingleOptionQuestion(UpdateSingleOptionQuestion(questionnaire.Id, isFilteredCombobox: true, randomizeAnswerOptions: true));

            Assert.That(questionnaire.QuestionnaireDocument.Find<IQuestion>(SingleQuestionId).RandomizeAnswerOptions, Is.False);
        }

        [Test]
        public void should_preserve_flag_when_question_is_cloned()
        {
            var questionnaire = CreateQuestionnaire();
            questionnaire.UpdateSingleOptionQuestion(UpdateSingleOptionQuestion(questionnaire.Id, isFilteredCombobox: false, randomizeAnswerOptions: true));

            var clone = (IQuestion)questionnaire.QuestionnaireDocument.Find<IQuestion>(SingleQuestionId).Clone();

            Assert.That(clone.RandomizeAnswerOptions, Is.True);
        }
    }
}
