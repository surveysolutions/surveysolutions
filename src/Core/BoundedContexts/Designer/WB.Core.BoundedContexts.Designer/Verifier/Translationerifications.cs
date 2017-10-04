using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Verifier
{
    public class TranslationVerifications : AbstractVerifier, IPartialVerifier
    {
        private readonly ITranslationsService translationService;

        public TranslationVerifications(ITranslationsService translationService)
        {
            this.translationService = translationService;
        }

        private IEnumerable<Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>>> ErrorsVerifiers => new[]
        {
            ErrorForTranslation(TranslationNameIsInvalid, "WB0256", VerificationMessages.WB0256_TranslationNameIsInvalid),
            ErrorForTranslation(TranslationHasEmptyContent, "WB0257", VerificationMessages.WB0257_TranslationHasEmptyContent),
            ErrorForTranslation(TranslationsHasDuplicatedNames, "WB0258", VerificationMessages.WB0258_TranslationsHaveDuplicatedNames),
        };

        private bool TranslationNameIsInvalid(Translation translation, MultiLanguageQuestionnaireDocument questionnaire)
        {
            var names = questionnaire.Translations.Select(t => t.Name);
            return names.All(name => string.IsNullOrWhiteSpace(name) || name.Length > 32);
        }

        private bool TranslationHasEmptyContent(Translation translation, MultiLanguageQuestionnaireDocument questionnaire)
        {
            var trans = this.translationService.Get(questionnaire.PublicKey, translation.Id);
            return trans?.IsEmpty() ?? true;
        }

        private bool TranslationsHasDuplicatedNames(Translation translation, MultiLanguageQuestionnaireDocument questionnaire)
        {
            var trimedTranslationName = translation.Name.Trim();
            var countNames = questionnaire.Questionnaire.Translations.Count(t => t.Name.Trim() == trimedTranslationName);
            return countNames > 1;
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> ErrorForTranslation(
            Func<Translation, MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
        {
            return questionnaire => questionnaire
                .Translations
                .Where(entity => hasError(entity, questionnaire))
                .Select(entity => QuestionnaireVerificationMessage.Error(code, message, QuestionnaireNodeReference.CreateForTranslation(entity.Id)));
        }

        public IEnumerable<QuestionnaireVerificationMessage> Verify(MultiLanguageQuestionnaireDocument multiLanguageQuestionnaireDocument)
        {
            var verificationMessagesByQuestionnaire = new List<QuestionnaireVerificationMessage>();
            foreach (var verifier in ErrorsVerifiers)
            {
                verificationMessagesByQuestionnaire.AddRange(verifier.Invoke(multiLanguageQuestionnaireDocument));
            }
            return verificationMessagesByQuestionnaire;
        }
    }
}