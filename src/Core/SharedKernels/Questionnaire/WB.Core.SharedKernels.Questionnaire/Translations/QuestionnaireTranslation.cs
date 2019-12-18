using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace WB.Core.SharedKernels.Questionnaire.Translations
{
    public class QuestionnaireTranslation : ITranslation
    {
        private readonly Dictionary<Guid, List<TranslationDto>> translations = new Dictionary<Guid, List<TranslationDto>>();

        public QuestionnaireTranslation(IEnumerable<TranslationDto> translationInstances)
        {
            if (this.translations == null) throw new ArgumentNullException(nameof(this.translations));

            foreach (var translation in translationInstances)
            {
                if (!this.translations.ContainsKey(translation.QuestionnaireEntityId))
                {
                    this.translations[translation.QuestionnaireEntityId] = new List<TranslationDto>();
                }

                this.translations[translation.QuestionnaireEntityId].Add(translation); 
            }
        }

        public string GetTitle(Guid entityId)
            => this.GetUniqueTranslationByType(entityId, TranslationType.Title);

        public string GetInstruction(Guid questionId)
            => this.GetUniqueTranslationByType(questionId, TranslationType.Instruction);

        public string GetAnswerOption(Guid questionId, string answerOptionValue, string answerParentValue)
            => this.GetTranslationByTypeAndIndex(questionId, $"{answerOptionValue}${answerParentValue}", TranslationType.OptionTitle);

        public string GetSpecialValue(Guid questionId, string answerOptionValue)
            => this.GetTranslationByTypeAndIndex(questionId, answerOptionValue, TranslationType.SpecialValue);

        public string GetValidationMessage(Guid entityId, int validationOneBasedIndex)
            => this.GetTranslationByTypeAndIndex(
                entityId, validationOneBasedIndex.ToString(), TranslationType.ValidationMessage);

        public string GetFixedRosterTitle(Guid rosterId, decimal fixedRosterTitleValue)
            => this.GetTranslationByTypeAndIndex(
                rosterId, fixedRosterTitleValue.ToString("F0", CultureInfo.InvariantCulture), TranslationType.FixedRosterTitle);

        public bool IsEmpty() => !this.translations.Any();

        public string GetCategoriesText(Guid categoriesId, int id, int? parentId)
            => this.GetTranslationByTypeAndIndex(categoriesId, $"{id}${parentId}", TranslationType.Categories);

        private string GetTranslationByTypeAndIndex(Guid questionOrCategoriesId, string answerOptionValue, TranslationType translationType) =>
            this.translations.ContainsKey(questionOrCategoriesId)
                ? this.translations[questionOrCategoriesId].SingleOrDefault(x => x.Type == translationType && x.TranslationIndex == answerOptionValue)?.Value
                : null;

        private string GetUniqueTranslationByType(Guid entityId, TranslationType translationType)
        {
            if (this.translations.ContainsKey(entityId))
            {
                var translationInstance = this.translations[entityId].SingleOrDefault(x => x.Type == translationType);
                return translationInstance?.Value;
            }

            return null;
        }
    }
}
