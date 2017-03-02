using System;
using System.Collections.Generic;
namespace WB.Core.SharedKernels.DataCollection
{
    public class ListAnswerRow
    {
        public ListAnswerRow(int value, string text)
        {
            this.Value = value;
            this.Text = text;
        }

        public int Value { get; }
        public string Text { get; }

        public override string ToString() => $"{Value} -> {Text}";
    }
}
