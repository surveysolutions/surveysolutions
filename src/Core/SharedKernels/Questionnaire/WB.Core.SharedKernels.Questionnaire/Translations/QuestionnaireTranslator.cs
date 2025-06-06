using System;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.Questionnaire.Translations
{
    public class QuestionnaireTranslator : IQuestionnaireTranslator
    {
        public QuestionnaireDocument Translate(QuestionnaireDocument originalDocument, ITranslation translation, bool useNullForEmptyTranslations = false)
        {
            var translatedDocument = originalDocument.Clone();

            TranslateTitle(translatedDocument, translation, useNullForEmptyTranslations);
            
            foreach (var entity in translatedDocument.Find<IComposite>())
            {
                TranslateEntityImpl(entity, translation, useNullForEmptyTranslations);
            }

            foreach (var criticalRule in translatedDocument.CriticalRules)
            {
                TranslateCriticalRule(criticalRule, translation, useNullForEmptyTranslations);
            }

            return translatedDocument;
        }

        public IComposite TranslateEntity(IComposite entity,
            ITranslation translation, 
            bool useNullForEmptyTranslations = false)
        {
            var translatedEntity = entity.Clone();
            TranslateEntityImpl(translatedEntity, translation, useNullForEmptyTranslations);
            return translatedEntity;
        }
        
        private void TranslateEntityImpl(IComposite entity,
            ITranslation translation, 
            bool useNullForEmptyTranslations = false)
        {
            var entityAsQuestion = entity as IQuestion;
            var entityAsValidatable = entity as IValidatable;
            var entityAsGroup = entity as IGroup;

            TranslateTitle(entity, translation, useNullForEmptyTranslations);

            if (entityAsQuestion != null)
            {
                TranslateInstruction(entityAsQuestion, translation, useNullForEmptyTranslations);

                if (entityAsQuestion.QuestionType == QuestionType.Numeric)
                    TranslateSpecialValues(entityAsQuestion, translation, useNullForEmptyTranslations);
                else
                    TranslateAnswerOptions(entityAsQuestion, translation, useNullForEmptyTranslations);
            }

            if (entityAsValidatable != null)
            {
                TranslateValidationMessages(entityAsValidatable, translation, useNullForEmptyTranslations);
            }

            if (entityAsGroup != null && entityAsGroup.IsRoster)
            {
                TranslateFixedRosterTitles(entityAsGroup, translation, useNullForEmptyTranslations);
            }
        }

        private static void TranslateTitle(IQuestionnaireEntity entity, ITranslation translation, bool useNullForEmptyTranslations)
        {
            entity.SetTitle(Translate(
                original: entity.GetTitle(),
                translated: translation.GetTitle(entity.PublicKey),
                useNullForEmptyTranslations) ?? string.Empty);
        }

        private static void TranslateInstruction(IQuestion question, ITranslation translation, bool useNullForEmptyTranslations)
        {
            question.Instructions = Translate(
                original: question.Instructions,
                translated: translation.GetInstruction(question.PublicKey),
                useNullForEmptyTranslations);
        }

        private static void TranslateCriticalRule(CriticalRule criticalRule, ITranslation translation, bool useNullForEmptyTranslations)
        {
            criticalRule.Message = Translate(
                original: criticalRule.Message,
                translated: translation.GetCriticalRuleMessage(criticalRule.Id),
                useNullForEmptyTranslations);
        }

        private static void TranslateAnswerOptions(IQuestion question, ITranslation translation, bool useNullForEmptyTranslations)
        {
            var categoriesId = (question as ICategoricalQuestion)?.CategoriesId;

            foreach (var answerOption in question.Answers)
            {
                if (categoriesId.HasValue)
                    TranslateCategories(categoriesId.Value, answerOption, translation, useNullForEmptyTranslations);
                else
                    TranslateAnswerOption(question.PublicKey, answerOption, translation, useNullForEmptyTranslations);
            }
        }

        private static void TranslateAnswerOption(Guid questionId, Answer answerOption, ITranslation translation, bool useNullForEmptyTranslations)
        {
            if(answerOption == null) throw new ArgumentException("Answer option must be not null.");
            
            answerOption.AnswerText = Translate(
                original: answerOption.AnswerText,
                translated: translation.GetAnswerOption(questionId, answerOption.AnswerValue, answerOption.ParentValue),
                useNullForEmptyTranslations
                ) ?? String.Empty;
        }

        private static void TranslateCategories(Guid categoriesId, Answer answerOption, ITranslation translation, bool useNullForEmptyTranslations) =>
            answerOption.AnswerText = Translate(
                original: answerOption.AnswerText,
                translated: translation.GetCategoriesText(categoriesId, (int)answerOption.GetParsedValue(), answerOption.GetParsedParentValue()),
                useNullForEmptyTranslations
                ) ?? String.Empty;

        private static void TranslateSpecialValues(IQuestion question, ITranslation translation, bool useNullForEmptyTranslations)
        {
            foreach (var answerOption in question.Answers)
            {
                TranslateSpecialValue(question.PublicKey, answerOption, translation, useNullForEmptyTranslations);
            }
        }

        private static void TranslateSpecialValue(Guid questionId, Answer answerOption, ITranslation translation, bool useNullForEmptyTranslations)
        {
            answerOption.AnswerText = Translate(
                original: answerOption.AnswerText,
                translated: translation.GetSpecialValue(questionId, answerOption.AnswerValue),
                useNullForEmptyTranslations
                ) ?? String.Empty;
        }

        private static void TranslateValidationMessages(IValidatable validatableEntity, ITranslation translation, bool useNullForEmptyTranslations)
        {
            for (int index = 0; index < validatableEntity.ValidationConditions.Count; index++)
            {
                var validationCondition = validatableEntity.ValidationConditions[index];
                TranslateValidationMessage(validatableEntity.PublicKey, index + 1, validationCondition, translation, useNullForEmptyTranslations);
            }
        }

        private static void TranslateValidationMessage(Guid validatableEntityId, int validationOneBasedIndex,
            ValidationCondition validation, ITranslation translation, bool useNullForEmptyTranslations)
        {
            validation.Message = Translate(
                original: validation.Message,
                translated: translation.GetValidationMessage(validatableEntityId, validationOneBasedIndex),
                useNullForEmptyTranslations: useNullForEmptyTranslations
                ) ?? String.Empty;
        }

        private static void TranslateFixedRosterTitles(IGroup roster, ITranslation translation, bool useNullForEmptyTranslations)
        {
            foreach (var fixedRosterTitle in roster.FixedRosterTitles)
            {
                TranslateFixedRosterTitle(roster.PublicKey, fixedRosterTitle, translation, useNullForEmptyTranslations);
            }
        }

        private static void TranslateFixedRosterTitle(Guid rosterId, FixedRosterTitle fixedRosterTitle, ITranslation translation, bool useNullForEmptyTranslations)
        {
            fixedRosterTitle.Title = Translate(
                original: fixedRosterTitle.Title,
                translated: translation.GetFixedRosterTitle(rosterId, fixedRosterTitle.Value),
                useNullForEmptyTranslations: useNullForEmptyTranslations
                );
        }

        private static string? Translate(string? original, string? translated, bool useNullForEmptyTranslations)
            => !string.IsNullOrWhiteSpace(translated) ? translated : (useNullForEmptyTranslations ? null : original);
    }
}
