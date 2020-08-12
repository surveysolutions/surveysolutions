using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Verifier;

namespace WB.UI.Designer.Models
{
    public class VerificationMessageError
    {
        public VerificationMessageError(IEnumerable<string>? compilationErrorMessages = null, List<QuestionnaireEntityExtendedReference>? references = null)
        {
            CompilationErrorMessages = compilationErrorMessages ?? new List<string>();
            References = references ?? new List<QuestionnaireEntityExtendedReference>();
        }

        public IEnumerable<string> CompilationErrorMessages { get; set; }
        public List<QuestionnaireEntityExtendedReference> References { get; set; }
    }
}
