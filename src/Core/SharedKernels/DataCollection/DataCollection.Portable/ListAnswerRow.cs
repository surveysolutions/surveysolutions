using System.Diagnostics;

namespace WB.Core.SharedKernels.DataCollection
{
    [DebuggerDisplay("{ToString()}")]
    public class TextListAnswerRow
    {
        public TextListAnswerRow(decimal value, string text)
        {
            this.Value = value;
            this.Text = text;
        }

        public decimal Value { get; }
        public string Text { get; }

        public override string ToString() => $"{Value} -> {Text}";
    }
}
