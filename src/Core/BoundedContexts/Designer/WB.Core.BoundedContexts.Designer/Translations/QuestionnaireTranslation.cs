using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.Questionnaire.Translator;

namespace WB.Core.BoundedContexts.Designer.Translations
{
    internal class QuestionnaireTranslation : IQuestionnaireTranslation
    {
        private readonly Dictionary<Guid, List<TranslationInstance>> translations = new Dictionary<Guid, List<TranslationInstance>>();

        public QuestionnaireTranslation(List<TranslationInstance> translationInstances)
        {
            if (this.translations == null) throw new ArgumentNullException(nameof(this.translations));


            foreach (var translation in translationInstances)
            {
                if (!this.translations.ContainsKey(translation.QuestionnaireEntityId))
                {
                    this.translations[translation.QuestionnaireEntityId] = new List<TranslationInstance>();
                }

                this.translations[translation.QuestionnaireEntityId].Add(translation); 
            }
        }

        public string GetTitle(Guid entityId)
        {
            return this.GetTranslationByType(entityId, TranslationType.Title);
        }

        public string GetInstruction(Guid questionId)
        {
            return this.GetTranslationByType(questionId, TranslationType.Instruction);
        }

        public string GetAnswerOption(Guid questionId, string answerOptionValue)
        {
            return this.GetTranslationByTypeAndIndex(questionId, answerOptionValue, TranslationType.OptionTitle);
        }

        public string GetValidationMessage(Guid entityId, int validationOneBasedIndex)
        {
            return this.GetTranslationByTypeAndIndex(entityId, validationOneBasedIndex.ToString(),
                TranslationType.ValidationMessage);
        }

        public string GetFixedRosterTitle(Guid rosterId, decimal fixedRosterTitleValue)
        {
            throw new NotImplementedException(); // TODO: ank
        }

        private string GetTranslationByTypeAndIndex(Guid questionId, string answerOptionValue, TranslationType translationType)
        {
            if (this.translations.ContainsKey(questionId))
            {
                var translationInstance = this.translations[questionId].SingleOrDefault(x => x.Type == translationType && x.TranslationIndex == answerOptionValue);
                return translationInstance?.Translation;
            }

            return null;
        }

        private string GetTranslationByType(Guid entityId, TranslationType translationType)
        {
            if (this.translations.ContainsKey(entityId))
            {
                var translationInstance = this.translations[entityId].SingleOrDefault(x => x.Type == translationType);
                return translationInstance?.Translation;
            }

            return null;
        }
    }
}