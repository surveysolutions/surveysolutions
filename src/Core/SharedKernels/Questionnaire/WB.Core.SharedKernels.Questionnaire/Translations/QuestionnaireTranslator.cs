using System;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.Questionnaire.Translations
{
    internal class QuestionnaireTranslator : IQuestionnaireTranslator
    {
        public QuestionnaireDocument Translate(QuestionnaireDocument originalDocument, ITranslation translation)
        {
            var translatedDocument = originalDocument.Clone();

            TranslateTitles(translatedDocument, translation);
            TranslateInstructions(translatedDocument, translation);
            TranslateAnswerOptions(translatedDocument, translation);
            TranslateValidationMessages(translatedDocument, translation);
            TranslateFixedRosterTitles(translatedDocument, translation);

            return translatedDocument;
        }

        private static void TranslateTitles(QuestionnaireDocument questionnaireDocument, ITranslation translation)
        {
            foreach (var entity in questionnaireDocument.Find<IQuestionnaireEntity>())
            {
                TranslateTitle(entity, translation);
            }
        }

        private static void TranslateInstructions(QuestionnaireDocument questionnaireDocument, ITranslation translation)
        {
            foreach (var question in questionnaireDocument.Find<IQuestion>())
            {
                TranslateInstruction(question, translation);
            }
        }

        private static void TranslateAnswerOptions(QuestionnaireDocument questionnaireDocument, ITranslation translation)
        {
            foreach (var question in questionnaireDocument.Find<IQuestion>())
            {
                foreach (var answerOption in question.Answers)
                {
                    TranslateAnswerOption(question.PublicKey, answerOption, translation);
                }
            }
        }

        private static void TranslateValidationMessages(QuestionnaireDocument questionnaireDocument, ITranslation translation)
        {
            foreach (var validatableEntity in questionnaireDocument.Find<IValidatable>())
            {
                for (int index = 0; index < validatableEntity.ValidationConditions.Count; index++)
                {
                    var validationCondition = validatableEntity.ValidationConditions[index];
                    TranslateValidationMessage(validatableEntity.PublicKey, index + 1, validationCondition, translation);
                }
            }
        }

        private static void TranslateFixedRosterTitles(QuestionnaireDocument questionnaireDocument, ITranslation translation)
        {
            foreach (var roster in questionnaireDocument.Find<IGroup>(group => @group.IsRoster))
            {
                foreach (var fixedRosterTitle in roster.FixedRosterTitles)
                {
                    TranslateFixedRosterTitle(roster.PublicKey, fixedRosterTitle, translation);
                }
            }
        }

        private static void TranslateTitle(IQuestionnaireEntity entity, ITranslation translation)
        {
            entity.SetTitle(Translate(
                original: entity.GetTitle(),
                translated: translation.GetTitle(entity.PublicKey)));
        }

        private static void TranslateInstruction(IQuestion question, ITranslation translation)
        {
            question.Instructions = Translate(
                original: question.Instructions,
                translated: translation.GetInstruction(question.PublicKey));
        }

        private static void TranslateAnswerOption(Guid questionId, Answer answerOption, ITranslation translation)
        {
            answerOption.AnswerText = Translate(
                original: answerOption.AnswerText,
                translated: translation.GetAnswerOption(questionId, answerOption.AnswerValue));
        }

        private static void TranslateValidationMessage(Guid validatableEntityId, int validationOneBasedIndex,
            ValidationCondition validation, ITranslation translation)
        {
            validation.Message = Translate(
                original: validation.Message,
                translated: translation.GetValidationMessage(validatableEntityId, validationOneBasedIndex));
        }

        private static void TranslateFixedRosterTitle(Guid rosterId, FixedRosterTitle fixedRosterTitle, ITranslation translation)
        {
            fixedRosterTitle.Title = Translate(
                original: fixedRosterTitle.Title,
                translated: translation.GetFixedRosterTitle(rosterId, fixedRosterTitle.Value));
        }

        private static string Translate(string original, string translated)
            => !string.IsNullOrWhiteSpace(translated) ? translated : original;
    }
}
