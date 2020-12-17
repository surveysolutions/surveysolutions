using System;

namespace StatData.Writers.Stata
{
    internal class StataSingleValueLabel : IComparable<StataSingleValueLabel>
    {
        public Int32 Value { get; set; }
        public string Text { get; set; }

        public int CompareTo(StataSingleValueLabel other)
        {
            return Value.CompareTo(other.Value);
        }
    }
}
