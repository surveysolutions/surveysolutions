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
    public class ImportQuestionnaireVerifier : AbstractVerifier, IQuestionnairePreVerifier
    {
        private IEnumerable<Func<ReadOnlyQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>>> ErrorsVerifiers => new[]
        {
            (Func<ReadOnlyQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>>)DuplicatePublicId,
            DuplicateVariableName,
        };

        private static IEnumerable<QuestionnaireVerificationMessage> DuplicateVariableName(ReadOnlyQuestionnaireDocument document)
        {
            HashSet<string> variables = new HashSet<string>();
            var elements = document.Questionnaire.Children
                .Cast<IComposite>()
                .TreeToEnumerable(c => c.Children);
            foreach (var entity in elements)
            {
                var variable = entity.GetVariable();
                if (!string.IsNullOrWhiteSpace(variable) && !variables.Add(variable))
                    yield return QuestionnaireVerificationMessage.Error(
                        "QM0007",
                        ImportExportVerificationMessages.QM0007,
                        new QuestionnaireEntityReference(
                            GetReferenceTypeByItemTypeAndId(document, entity.PublicKey, entity.GetType()),
                            entity.PublicKey));
            }
        }

        private static IEnumerable<QuestionnaireVerificationMessage> DuplicatePublicId(ReadOnlyQuestionnaireDocument document)
        {
            HashSet<Guid> guids = new HashSet<Guid>();
            var elements = document.Questionnaire.Children
                .Cast<IComposite>()
                .TreeToEnumerable(c => c.Children);
            foreach (var entity in elements)
            {
                if (!guids.Add(entity.PublicKey))
                    yield return QuestionnaireVerificationMessage.Error(
                        "QM0006",
                        ImportExportVerificationMessages.QM0006,
                        new QuestionnaireEntityReference(
                            GetReferenceTypeByItemTypeAndId(document, entity.PublicKey, entity.GetType()),
                            entity.PublicKey));
            }
        }
        
        public IEnumerable<QuestionnaireVerificationMessage> Verify(ReadOnlyQuestionnaireDocument multiLanguageQuestionnaireDocument)
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
