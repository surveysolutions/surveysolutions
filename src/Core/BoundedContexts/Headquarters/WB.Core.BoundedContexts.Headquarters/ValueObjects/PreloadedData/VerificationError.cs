using System.Diagnostics;

namespace WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData
{
    [DebuggerDisplay("{ToString()}")]
    public class VerificationError
    {
        public string Column { get; set; }
        public int Row { get; set; }
        public string FileName { get; set; }

        //old school style compatibility
        public string Code { get; set; }
        public string Message { get; set; }
        public string Value { get; set; }

        public override string ToString() => $"{this.Code}: {this.Message}";
    }
}