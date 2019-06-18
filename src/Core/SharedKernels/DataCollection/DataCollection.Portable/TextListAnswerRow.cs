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

        public int Value { get; set; }
        public string Text { get; set; }

        [Obsolete("Compatibility with v 5.20")]
        public int Item1 => Value;

        [Obsolete("Compatibility with v 5.20")]
        public string Item2 => Text;

        public override string ToString() => $"{Value} -> {Text}";

        protected bool Equals(TextListAnswerRow other)
        {
            return Value == other.Value && string.Equals(Text, other.Text);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Value * 397) ^ (Text != null ? Text.GetHashCode() : 0);
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TextListAnswerRow) obj);
        }
    }
}
