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
                (
                    code : x.Key.Code,
                    message : x.Key.Message,
                    isGroupedMessage : true,
                    errors : x.Select(g => new VerificationMessageError
                    (
                        references : g.References.Select(reference => reference.ExtendedReference(questionnaire)).ToList(),
                        compilationErrorMessages : g.CompilationErrorMessages 
                    )).ToList()
                )).ToList();

            errors.AddRange(verificationMessages
                .Where(x => x.References.Count != 1).Select(x => new VerificationMessage
                (
                    code : x.Code,
                    message : x.Message,
                    isGroupedMessage : false,
                    errors : new List<VerificationMessageError>
                    {
                        new VerificationMessageError
                        (
                            compilationErrorMessages : x.CompilationErrorMessages,
                            references : x.References.Select(reference => reference.ExtendedReference(questionnaire)).ToList()
                        )
                    }
                )));

            return errors.OrderBy(x => x.Code).ToArray();
        }
    }
}
