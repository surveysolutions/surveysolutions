using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace WB.Core.SharedKernels.Questionnaire.Translations
{
    public class QuestionnaireTranslation : ITranslation
    {
        private readonly Dictionary<Guid, List<TranslationDto>> translations = new Dictionary<Guid, List<TranslationDto>>();

        public QuestionnaireTranslation(List<TranslationDto> translationInstances)
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
            => this.GetTranslationByType(entityId, TranslationType.Title);

        public string GetInstruction(Guid questionId)
            => this.GetTranslationByType(questionId, TranslationType.Instruction);

        public string GetAnswerOption(Guid questionId, string answerOptionValue)
            => this.GetTranslationByTypeAndIndex(questionId, answerOptionValue, TranslationType.OptionTitle);

        public string GetValidationMessage(Guid entityId, int validationOneBasedIndex)
            => this.GetTranslationByTypeAndIndex(
                entityId, validationOneBasedIndex.ToString(), TranslationType.ValidationMessage);

        public string GetFixedRosterTitle(Guid rosterId, decimal fixedRosterTitleValue)
            => this.GetTranslationByTypeAndIndex(
                rosterId, fixedRosterTitleValue.ToString(CultureInfo.InvariantCulture), TranslationType.FixedRosterTitle);

        public bool IsEmpty() => !this.translations.Any();

        private string GetTranslationByTypeAndIndex(Guid questionId, string answerOptionValue, TranslationType translationType)
        {
            if (this.translations.ContainsKey(questionId))
            {
                var translationInstance = this.translations[questionId].SingleOrDefault(x => x.Type == translationType && x.TranslationIndex == answerOptionValue);
                return translationInstance?.Value;
            }

            return null;
        }

        private string GetTranslationByType(Guid entityId, TranslationType translationType)
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