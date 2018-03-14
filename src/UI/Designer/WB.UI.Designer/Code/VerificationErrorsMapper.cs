using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.UI.Designer.Models;

namespace WB.UI.Designer.Code
{
    public interface IVerificationErrorsMapper
    {
        VerificationMessage[] EnrichVerificationErrors(QuestionnaireVerificationMessage[] verificationMessages, ReadOnlyQuestionnaireDocument questionnaire);
    }

    public class VerificationErrorsMapper : IVerificationErrorsMapper
    {
        public VerificationMessage[] EnrichVerificationErrors(QuestionnaireVerificationMessage[] verificationMessages, ReadOnlyQuestionnaireDocument questionnaire)
        {
            var errors = verificationMessages
                .Where(x => x.References.Count == 1)
                .GroupBy(x => new { x.Code, x.Message })
                .Select(x => new VerificationMessage
                {
                    Code = x.Key.Code,
                    Message = x.Key.Message,
                    IsGroupedMessage = true,
                    Errors = x.Select(g => new VerificationMessageError
                    {
                        References = g.References.Select(reference => reference.ExtendedReference(questionnaire)).ToList(),
                        CompilationErrorMessages = g.CompilationErrorMessages
                    }).ToList()
                }).ToList();

            errors.AddRange(verificationMessages
                .Where(x => x.References.Count != 1).Select(x => new VerificationMessage
                {
                    Code = x.Code,
                    Message = x.Message,
                    IsGroupedMessage = false,
                    Errors = new List<VerificationMessageError>
                    {
                        new VerificationMessageError
                        {
                            CompilationErrorMessages = x.CompilationErrorMessages,
                            References = x.References.Select(reference => reference.ExtendedReference(questionnaire)).ToList()
                        }
                    }
                }));

            return errors.OrderBy(x => x.Code).ToArray();
        }
    }
}
