using System.Collections.Generic;

namespace WB.UI.Designer.Models
{
    public class VerificationMessageError
    {
        public IEnumerable<string> CompilationErrorMessages { get; set; }
        public List<VerificationReferenceEnriched> References { get; set; }
    }
}