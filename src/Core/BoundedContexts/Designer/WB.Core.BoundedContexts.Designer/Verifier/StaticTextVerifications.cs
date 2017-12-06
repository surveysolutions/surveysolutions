using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Core.BoundedContexts.Designer.Verifier
{
    public class StaticTextVerifications : AbstractVerifier, IPartialVerifier
    {
        private IEnumerable<Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>>> ErrorsVerifiers => new[]
        {
            ErrorForTranslation<IStaticText>(StaticTextIsEmpty, "WB0071", VerificationMessages.WB0071_StaticTextIsEmpty),
            Error<IStaticText>(StaticTextRefersAbsentAttachment, "WB0095", VerificationMessages.WB0095_StaticTextRefersAbsentAttachment),
        };

        private static bool StaticTextIsEmpty(IStaticText staticText, MultiLanguageQuestionnaireDocument document)
        {
            return string.IsNullOrWhiteSpace(staticText.Text);
        }

        private static bool StaticTextRefersAbsentAttachment(IStaticText staticText, MultiLanguageQuestionnaireDocument document)
        {
            if (string.IsNullOrWhiteSpace(staticText.AttachmentName))
                return false;

            return document.Attachments.All(x => x.Name != staticText.AttachmentName);
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Error<TEntity>(
            Func<TEntity, MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
            where TEntity : class, IComposite
        {
            return questionnaire =>
                questionnaire
                    .Find<TEntity>(entity => hasError(entity, questionnaire))
                    .Select(entity => QuestionnaireVerificationMessage.Error(code, message, CreateReference(entity)));
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> ErrorForTranslation<TEntity>(
            Func<TEntity, MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
            where TEntity : class, IComposite
        {
            return questionnaire =>
                questionnaire
                    .FindWithTranslations<TEntity>(entity => hasError(entity, questionnaire))
                    .Select(translatedEntity =>
                    {
                        var translationMessage = translatedEntity.TranslationName == null
                            ? message
                            : translatedEntity.TranslationName + ": " + message;
                        var questionnaireVerificationReference = CreateReference(translatedEntity.Entity);
                        return QuestionnaireVerificationMessage.Error(code, translationMessage, questionnaireVerificationReference);
                    });
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