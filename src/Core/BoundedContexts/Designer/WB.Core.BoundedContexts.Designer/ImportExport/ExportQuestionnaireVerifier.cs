using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Verifier;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Core.BoundedContexts.Designer.ImportExport
{
    public class ExportQuestionnaireVerifier : AbstractVerifier, IPartialVerifier
    {
        private IEnumerable<Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>>> ErrorsVerifiers => new[]
        {
            Error<IQuestion>("WB0392", LinkedToQuestionMustHaveVariable, VerificationMessages.WB0392),
            Error<IQuestion>("WB0393", LinkedToRosterMustHaveVariable, VerificationMessages.WB0393),
            Error<IQuestion>("WB0394", CascadeFromQuestionMustHaveVariable, VerificationMessages.WB0394),
            Error<IGroup>("WB0395", RosterTriggerMustHaveVariable, VerificationMessages.WB0395),
            Error<IGroup>("WB0396", RosterTitleMustHaveVariable, VerificationMessages.WB0396),
        };

        private IQuestionnaireEntity? RosterTriggerMustHaveVariable(IGroup roster, MultiLanguageQuestionnaireDocument document)
        {
            if (roster.IsRoster && roster.RosterSizeQuestionId.HasValue)
            {
                var rosterTrigger = document.Find<IQuestion>(roster.RosterSizeQuestionId.Value);
                var variableNameIsEmpty = rosterTrigger?.VariableName?.Trim().IsNullOrEmpty() ?? true;
                if (variableNameIsEmpty)
                    return rosterTrigger;
            }

            return null;
        }
        
        private IQuestionnaireEntity? RosterTitleMustHaveVariable(IGroup roster, MultiLanguageQuestionnaireDocument document)
        {
            if (roster.IsRoster && roster.RosterTitleQuestionId.HasValue)
            {
                var rosterTitle = document.Find<IQuestion>(roster.RosterTitleQuestionId.Value);
                var variableNameIsEmpty = rosterTitle?.VariableName?.Trim().IsNullOrEmpty() ?? true;
                if (variableNameIsEmpty)
                    return rosterTitle;
            }

            return null;
        }

        private IQuestionnaireEntity? LinkedToQuestionMustHaveVariable(IQuestion question, MultiLanguageQuestionnaireDocument document)
        {
            if (question.LinkedToQuestionId.HasValue)
            {
                var linkedTo = document.Find<IQuestion>(question.LinkedToQuestionId.Value);
                var variableNameIsEmpty = linkedTo?.VariableName?.Trim().IsNullOrEmpty() ?? true;
                if (variableNameIsEmpty)
                    return linkedTo;
            }

            return null;
        }

        private IQuestionnaireEntity? LinkedToRosterMustHaveVariable(IQuestion question, MultiLanguageQuestionnaireDocument document)
        {
            if (question.LinkedToRosterId.HasValue)
            {
                var linkedTo = document.Find<IGroup>(question.LinkedToRosterId.Value);
                var variableNameIsEmpty = linkedTo?.VariableName?.Trim().IsNullOrEmpty() ?? true;
                if (variableNameIsEmpty)
                    return linkedTo;
            }

            return null;
        }

        private IQuestionnaireEntity? CascadeFromQuestionMustHaveVariable(IQuestion question, MultiLanguageQuestionnaireDocument document)
        {
            if (question.CascadeFromQuestionId.HasValue)
            {
                var cascadeFrom = document.Find<IQuestion>(question.CascadeFromQuestionId.Value);
                var variableNameIsEmpty = cascadeFrom?.VariableName?.Trim().IsNullOrEmpty() ?? true;
                if (variableNameIsEmpty)
                    return cascadeFrom;
            }

            return null;
        }
        public IEnumerable<QuestionnaireVerificationMessage> Verify(MultiLanguageQuestionnaireDocument multiLanguageQuestionnaireDocument)
        {
            var verificationMessagesByQuestionnaire = new List<QuestionnaireVerificationMessage>();
            foreach (var verifier in this.ErrorsVerifiers)
            {
                verificationMessagesByQuestionnaire.AddRange(verifier.Invoke(multiLanguageQuestionnaireDocument));
            }
            return verificationMessagesByQuestionnaire;
        }
    }
}