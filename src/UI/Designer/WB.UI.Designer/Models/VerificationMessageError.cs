using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Verifier;

namespace WB.UI.Designer.Models
{
    public class VerificationMessageError
    {
        public IEnumerable<string> CompilationErrorMessages { get; set; }
        public List<QuestionnaireEntityExtendedReference> References { get; set; }
    }
}
