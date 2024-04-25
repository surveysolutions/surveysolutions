using System;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.Questionnaire.Translations
{
    public class QuestionnaireTranslator : IQuestionnaireTranslator
    {
        public QuestionnaireDocument Translate(QuestionnaireDocument originalDocument, ITranslation translation)
        {
            var translatedDocument = originalDocument.Clone();

            TranslateTitle(translatedDocument, translation);
            
            foreach (var entity in translatedDocument.Find<IQuestionnaireEntity>())
            {
                var entityAsQuestion = entity as IQuestion;
                var entityAsValidatable = entity as IValidatable;
                var entityAsGroup = entity as IGroup;

                TranslateTitle(entity, translation);

                if (entityAsQuestion != null)
                {
                    TranslateInstruction(entityAsQuestion, translation);

                    if (entityAsQuestion.QuestionType == QuestionType.Numeric)
                        TranslateSpecialValues(entityAsQuestion, translation);
                    else
                        TranslateAnswerOptions(entityAsQuestion, translation);
                }

                if (entityAsValidatable != null)
                {
                    TranslateValidationMessages(entityAsValidatable, translation);
                }

                if (entityAsGroup != null && entityAsGroup.IsRoster)
                {
                    TranslateFixedRosterTitles(entityAsGroup, translation);
                }
            }

            foreach (var criticalityCondition in translatedDocument.CriticalRules)
            {
                TranslateCriticalityCondition(criticalityCondition, translation);
            }

            return translatedDocument;
        }

        private static void TranslateTitle(IQuestionnaireEntity entity, ITranslation translation)
        {
            entity.SetTitle(Translate(
                original: entity.GetTitle(),
                translated: translation.GetTitle(entity.PublicKey)) ?? string.Empty);
        }

        private static void TranslateInstruction(IQuestion question, ITranslation translation)
        {
            question.Instructions = Translate(
                original: question.Instructions,
                translated: translation.GetInstruction(question.PublicKey));
        }

        private static void TranslateCriticalityCondition(CriticalRule criticalRule, ITranslation translation)
        {
            criticalRule.Message = Translate(
                original: criticalRule.Message,
                translated: translation.GetCriticalRuleMessage(criticalRule.Id));
        }

        private static void TranslateAnswerOptions(IQuestion question, ITranslation translation)
        {
            var categoriesId = (question as ICategoricalQuestion)?.CategoriesId;

            foreach (var answerOption in question.Answers)
            {
                if (categoriesId.HasValue)
                    TranslateCategories(categoriesId.Value, answerOption, translation);
                else
                    TranslateAnswerOption(question.PublicKey, answerOption, translation);
            }
        }

        private static void TranslateAnswerOption(Guid questionId, Answer answerOption, ITranslation translation)
        {
            if(answerOption == null) throw new ArgumentException("Answer option must be not null.");
            
            answerOption.AnswerText = Translate(
                original: answerOption.AnswerText,
                translated: translation.GetAnswerOption(questionId, answerOption.AnswerValue, answerOption.ParentValue))
                                      ?? String.Empty;
        }

        private static void TranslateCategories(Guid categoriesId, Answer answerOption, ITranslation translation) =>
            answerOption.AnswerText = Translate(
                original: answerOption.AnswerText,
                translated: translation.GetCategoriesText(categoriesId, (int)answerOption.GetParsedValue(), answerOption.GetParsedParentValue())) 
                                      ?? String.Empty;

        private static void TranslateSpecialValues(IQuestion question, ITranslation translation)
        {
            foreach (var answerOption in question.Answers)
            {
                TranslateSpecialValue(question.PublicKey, answerOption, translation);
            }
        }

        private static void TranslateSpecialValue(Guid questionId, Answer answerOption, ITranslation translation)
        {
            answerOption.AnswerText = Translate(
                original: answerOption.AnswerText,
                translated: translation.GetSpecialValue(questionId, answerOption.AnswerValue)) 
                                      ?? String.Empty;
        }

        private static void TranslateValidationMessages(IValidatable validatableEntity, ITranslation translation)
        {
            for (int index = 0; index < validatableEntity.ValidationConditions.Count; index++)
            {
                var validationCondition = validatableEntity.ValidationConditions[index];
                TranslateValidationMessage(validatableEntity.PublicKey, index + 1, validationCondition, translation);
            }
        }

        private static void TranslateValidationMessage(Guid validatableEntityId, int validationOneBasedIndex,
            ValidationCondition validation, ITranslation translation)
        {
            validation.Message = Translate(
                original: validation.Message,
                translated: translation.GetValidationMessage(validatableEntityId, validationOneBasedIndex)) ?? String.Empty;
        }

        private static void TranslateFixedRosterTitles(IGroup roster, ITranslation translation)
        {
            foreach (var fixedRosterTitle in roster.FixedRosterTitles)
            {
                TranslateFixedRosterTitle(roster.PublicKey, fixedRosterTitle, translation);
            }
        }

        private static void TranslateFixedRosterTitle(Guid rosterId, FixedRosterTitle fixedRosterTitle, ITranslation translation)
        {
            fixedRosterTitle.Title = Translate(
                original: fixedRosterTitle.Title,
                translated: translation.GetFixedRosterTitle(rosterId, fixedRosterTitle.Value));
        }

        private static string? Translate(string? original, string? translated)
            => !string.IsNullOrWhiteSpace(translated) ? translated : original;
    }
}
