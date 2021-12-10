using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection
{
    [DebuggerDisplay("{ToString()}")]
    public class CheckedYesNoAnswerOption
    {
        protected bool Equals(CheckedYesNoAnswerOption other)
        {
            return Value == other.Value && Yes == other.Yes;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Value * 397) ^ Yes.GetHashCode();
            }
        }

        public CheckedYesNoAnswerOption(int value, bool yes)
        {
            this.Value = value;
            this.Yes = yes;
        }

        public int Value { get; set; }
        public bool Yes { get; set; }
        public bool No => !Yes;

        public override string ToString() => $"{this.Value} -> {(this.Yes ? "Yes" : "No")}";

        public static CheckedYesNoAnswerOption Parse(string s)
        {
            string[] stringParts = s.Split(new[] {"->"}, StringSplitOptions.RemoveEmptyEntries);

            return new CheckedYesNoAnswerOption(
                int.Parse(stringParts[0], CultureInfo.InvariantCulture),
                stringParts[1].Trim().ToLower() == "yes");
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CheckedYesNoAnswerOption) obj);
        }
    }

    // new type with interger arrays for ExpressionStorage
    public class YesNoAndAnswersMissings
    {
        public YesNoAndAnswersMissings(IEnumerable<int> allOptionCodes, IReadOnlyCollection<CheckedYesNoAnswerOption> checkedOptions = null)
        {
            this.allOptionCodesEnumerable = allOptionCodes;
            this.checkedOptions = checkedOptions;
            this.selectedYesCodes = null;
            this.selectedNoCodes = null;
            this.missingCodes = null;
            this.allOptionCodes = null;
        }

        private int[] allOptionCodes;
        private int[] selectedYesCodes;
        private int[] selectedNoCodes;
        private int[] missingCodes;

        private readonly IReadOnlyCollection<CheckedYesNoAnswerOption> checkedOptions;
        private readonly IEnumerable<int> allOptionCodesEnumerable;

        public int[] All => allOptionCodes ?? (this.allOptionCodes = this.allOptionCodesEnumerable.ToArray());
        public int[] Yes => selectedYesCodes ?? (selectedYesCodes = this.checkedOptions?.Where(c => c.Yes).Select(c => c.Value).ToArray() ?? Array.Empty<int>());
        public int[] No => selectedNoCodes ?? (selectedNoCodes = this.checkedOptions?.Where(c => c.No).Select(c => c.Value).ToArray() ?? Array.Empty<int>());
        public int[] Missing => missingCodes 
            ?? (missingCodes = this.All.Except(this.checkedOptions?.Select(c => c.Value) ?? Array.Empty<int>()).ToArray());

        public bool? this[int code]
        {
            get
            {
                if (this.No.Contains(code))
                {
                    return false;
                }

                if (this.Yes.Contains(code))
                {
                    return true;
                }

                if (!this.All.Contains(code))
                {
                    throw new IndexOutOfRangeException("Option with code {code} is absent");
                }

                return null;
            }
        }
    }
}
