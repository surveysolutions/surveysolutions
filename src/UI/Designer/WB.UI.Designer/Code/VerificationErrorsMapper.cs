using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;
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
                if (reference.Type == QuestionnaireVerificationReferenceType.Group)
                {
                    var group = questionnaireDocument.Find<IGroup>(reference.Id);

                    yield return new VerificationReference
                    {
                        Id = reference.Id,
                        Type = reference.Type,
                        Title = group.Title
                    };
                }
                else
                {
                    var question = questionnaireDocument.Find<IQuestion>(reference.Id);
                    yield return new VerificationReference
                    {
                        Id = reference.Id,
                        Type = reference.Type,
                        Title = question.QuestionText
                    };
                }
            }
        }
    }
}