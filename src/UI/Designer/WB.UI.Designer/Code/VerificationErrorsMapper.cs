using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;
using WB.UI.Designer.BootstrapSupport;
using WB.UI.Designer.Models;

namespace WB.UI.Designer.Code
{
    public interface IVerificationErrorsMapper
    {
        VerificationError[] EnrichVerificationErrors(QuestionnaireVerificationError[] verificationErrors, QuestionnaireDocument questionnaireDocument);
    }

    public class VerificationErrorsMapper : IVerificationErrorsMapper
    {
        public VerificationError[] EnrichVerificationErrors(QuestionnaireVerificationError[] verificationErrors, QuestionnaireDocument questionnaireDocument)
        {
            var errors = verificationErrors.Select(x => new VerificationError
            {
                Code = x.Code,
                Message = x.Message,
                References = GetEnrichedReferences(x.References, questionnaireDocument).ToList()
            }).ToArray();

            return errors;
        }

        private IEnumerable<VerificationReference> GetEnrichedReferences(
            IEnumerable<QuestionnaireVerificationReference> references,
            QuestionnaireDocument questionnaireDocument)
        {
            foreach (var reference in references)
            {
                var item = questionnaireDocument.Find<IComposite>(reference.Id);
                var parent = item;
                while (parent != null)
                {
                    IComposite grandParent = parent.GetParent();
                    if (grandParent == null || grandParent.GetParent() == null)
                    {
                        break;
                    }
                    else
                    {
                        parent = grandParent;
                    }
                }

                if (reference.Type == QuestionnaireVerificationReferenceType.Group)
                {
                    var group = questionnaireDocument.Find<IGroup>(reference.Id);

                    yield return new VerificationReference
                    {
                        ItemId = reference.Id.FormatGuid(),
                        Type = group.IsRoster ? QuestionnaireVerificationReferenceType.Roster : reference.Type,
                        Title = group.Title,
                        ChapterId = Monads.Maybe(() => parent.PublicKey.FormatGuid())
                    };
                }
                else if (reference.Type == QuestionnaireVerificationReferenceType.StaticText)
                {
                    var staticText = questionnaireDocument.Find<IStaticText>(reference.Id);

                    yield return new VerificationReference
                    {
                        ItemId = reference.Id.FormatGuid(),
                        Type = reference.Type,
                        Title = string.IsNullOrEmpty(staticText.Text) ? "static text" : staticText.Text,
                        ChapterId = Monads.Maybe(() => parent.PublicKey.FormatGuid())
                    };
                }
                else
                {
                    var question = questionnaireDocument.Find<IQuestion>(reference.Id);

                    yield return new VerificationReference
                    {
                        ItemId = reference.Id.FormatGuid(),
                        Type = reference.Type,
                        Title = question.QuestionText,
                        ChapterId = Monads.Maybe(() => parent.PublicKey.FormatGuid())
                    };
                }
            }
        }
    }
}