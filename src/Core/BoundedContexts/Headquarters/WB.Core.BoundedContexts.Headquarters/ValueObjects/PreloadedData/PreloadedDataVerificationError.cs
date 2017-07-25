using System.Collections.Generic;
using System.Linq;

namespace WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData
{
    public class PanelImportVerificationError
    {
        public PanelImportVerificationError(string code, string message, params PreloadedDataVerificationReference[] references)
        {
            this.Code = code;
            this.Message = message;
            this.References = references.ToList();
        }

        public string Code { get; }
        public string Message { get; }
        public IEnumerable<PreloadedDataVerificationReference> References { get; }

        public override string ToString() => $"{this.Code}: {this.Message}";
    }
}
