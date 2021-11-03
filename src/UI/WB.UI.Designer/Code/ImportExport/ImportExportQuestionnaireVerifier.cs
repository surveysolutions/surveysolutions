using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Verifier;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.UI.Designer.Code.ImportExport
{
    public class ImportExportQuestionnaireVerifier : AbstractVerifier, IPartialVerifier
    {
        private IEnumerable<Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>>> ErrorsVerifiers => new[]
        {
            Error<IQuestion>("WB0500", LinkedToQuestionMustHaveVariable, VerificationMessages.WB0030_PrefilledQuestionCantBeInsideOfRoster),
            Error<IQuestion>("WB0501", LinkedToRosterMustHaveVariable, VerificationMessages.WB0030_PrefilledQuestionCantBeInsideOfRoster),
            Error<IQuestion>("WB0502", CascadeFromQuestionMustHaveVariable, VerificationMessages.WB0030_PrefilledQuestionCantBeInsideOfRoster),
            Error<IGroup>("WB0503", RosterTriggerMustHaveVariable, VerificationMessages.WB0030_PrefilledQuestionCantBeInsideOfRoster),
            Error<IGroup>("WB0504", RosterTitleMustHaveVariable, VerificationMessages.WB0030_PrefilledQuestionCantBeInsideOfRoster),
        };

        private bool RosterTriggerMustHaveVariable(IGroup roster, MultiLanguageQuestionnaireDocument document)
        {
            if (roster.IsRoster && roster.RosterSizeQuestionId.HasValue)
            {
                var rosterTrigger = document.Find<IQuestion>(roster.RosterSizeQuestionId.Value);
                return rosterTrigger?.VariableName?.Trim().IsNullOrEmpty() ?? true;
            }

            return false;
        }
        
        private bool RosterTitleMustHaveVariable(IGroup roster, MultiLanguageQuestionnaireDocument document)
        {
            if (roster.IsRoster && roster.RosterTitleQuestionId.HasValue)
            {
                var rosterTitle = document.Find<IQuestion>(roster.RosterTitleQuestionId.Value);
                return rosterTitle?.VariableName?.Trim().IsNullOrEmpty() ?? true;
            }

            return false;
        }

        private bool LinkedToQuestionMustHaveVariable(IQuestion question, MultiLanguageQuestionnaireDocument document)
        {
            if (question.LinkedToQuestionId.HasValue)
            {
                var linkedTo = document.Find<IQuestion>(question.LinkedToQuestionId.Value);
                return linkedTo?.VariableName?.Trim().IsNullOrEmpty() ?? true;
            }

            return false;
        }

        private bool LinkedToRosterMustHaveVariable(IQuestion question, MultiLanguageQuestionnaireDocument document)
        {
            if (question.LinkedToRosterId.HasValue)
            {
                var linkedTo = document.Find<IGroup>(question.LinkedToRosterId.Value);
                return linkedTo?.VariableName?.Trim().IsNullOrEmpty() ?? true;
            }

            return false;
        }

        private bool CascadeFromQuestionMustHaveVariable(IQuestion question, MultiLanguageQuestionnaireDocument document)
        {
            if (question.CascadeFromQuestionId.HasValue)
            {
                var cascadeFrom = document.Find<IQuestion>(question.CascadeFromQuestionId.Value);
                return cascadeFrom?.VariableName?.Trim().IsNullOrEmpty() ?? true;
            }

            return false;
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