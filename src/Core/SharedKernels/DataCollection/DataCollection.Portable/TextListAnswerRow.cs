using System;
using System.Diagnostics;

namespace WB.Core.SharedKernels.DataCollection
{
    [DebuggerDisplay("{ToString()}")]
    public class TextListAnswerRow
    {
        public TextListAnswerRow(int value, string text)
        {
            this.Value = value;
            this.Text = text;
        }

        public int Value { get; }
        public string Text { get; }

        [Obsolete("Compatibility with v 5.20")]
        public int Item1 => Value;

        [Obsolete("Compatibility with v 5.20")]
        public string Item2 => Text;

        public override string ToString() => $"{Value} -> {Text}";
    }
}