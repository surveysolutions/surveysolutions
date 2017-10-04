using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Verifier
{
    public class VariableVerifications : AbstractVerifier, IPartialVerifier
    {
        private readonly ISubstitutionService substitutionService;

        public VariableVerifications(ISubstitutionService substitutionService)
        {
            this.substitutionService = substitutionService;
        }

        private IEnumerable<Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>>> ErrorsVerifiers => new[]
        {
            Error<IVariable>(VariableHasInvalidName, "WB0112", VerificationMessages.WB0112_VariableHasInvalidName),
            Critical<IVariable>(VariableHasEmptyVariableName, "WB0113", VerificationMessages.WB0113_VariableHasEmptyVariableName),
            Critical<IVariable>(VariableHasEmptyExpression, "WB0004", VerificationMessages.WB0004_VariableHasEmptyExpression),
            ErrorForTranslation<IVariable>(this.IsNotSupportSubstitution, "WB0268", VerificationMessages.WB0268_DoesNotSupportSubstitution),
        };

        private bool IsNotSupportSubstitution(IVariable variable, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return substitutionService.GetAllSubstitutionVariableNames(variable.Label).Length > 0;
        }

        private bool VariableHasInvalidName(IVariable variable, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (string.IsNullOrWhiteSpace(variable.Name))
                return false;
            return !IsVariableNameValid(variable.Name);
        }

        private static bool VariableHasEmptyExpression(IVariable variable, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return string.IsNullOrWhiteSpace(variable.Expression);
        }

        private static bool VariableHasEmptyVariableName(IVariable variable, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return string.IsNullOrWhiteSpace(variable.Name);
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Critical<TEntity>(
            Func<TEntity, MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
            where TEntity : class, IComposite
        {
            return questionnaire =>
                questionnaire
                    .Find<TEntity>(entity => hasError(entity, questionnaire))
                    .Select(entity => QuestionnaireVerificationMessage.Critical(code, message, CreateReference(entity)));
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