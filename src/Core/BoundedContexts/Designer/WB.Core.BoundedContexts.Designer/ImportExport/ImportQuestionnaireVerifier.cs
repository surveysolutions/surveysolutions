using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Verifier;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Core.BoundedContexts.Designer.ImportExport
{
    public class ImportQuestionnaireVerifier : AbstractVerifier, IPartialVerifier
    {
        private IEnumerable<Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>>> ErrorsVerifiers => new[]
        {
            (Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>>)DuplicatePublicId,
            DuplicateVariableName,
        };

        private static IEnumerable<QuestionnaireVerificationMessage> DuplicateVariableName(MultiLanguageQuestionnaireDocument document)
        {
            HashSet<string> variables = new HashSet<string>();
            var elements = document.Questionnaire.Questionnaire.Children
                .Cast<IComposite>()
                .TreeToEnumerable(c => c.Children);
            foreach (var entity in elements)
            {
                var variable = entity.GetVariable();
                if (variable != null && variables.Add(variable))
                    yield return QuestionnaireVerificationMessage.Error(
                        "WB0398",
                        VerificationMessages.WB0398,
                        new QuestionnaireEntityReference(
                            GetReferenceTypeByItemTypeAndId(document, entity.PublicKey, entity.GetType()),
                            entity.PublicKey));
            }
        }

        private static IEnumerable<QuestionnaireVerificationMessage> DuplicatePublicId(MultiLanguageQuestionnaireDocument document)
        {
            HashSet<Guid> guids = new HashSet<Guid>();
            var elements = document.Questionnaire.Questionnaire.Children
                .Cast<IComposite>()
                .TreeToEnumerable(c => c.Children);
            foreach (var entity in elements)
            {
                if (guids.Add(entity.PublicKey))
                    yield return QuestionnaireVerificationMessage.Error(
                        "WB0397",
                        VerificationMessages.WB0397,
                        new QuestionnaireEntityReference(
                            GetReferenceTypeByItemTypeAndId(document, entity.PublicKey, entity.GetType()),
                            entity.PublicKey));
            }
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