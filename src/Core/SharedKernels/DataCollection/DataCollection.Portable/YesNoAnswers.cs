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
    }

    // new type with interger arrays for ExpressionStorage
    public class YesNoAndAnswersMissings
    {
        public YesNoAndAnswersMissings(IEnumerable<int> allOptionCodes, IReadOnlyCollection<CheckedYesNoAnswerOption> checkedOptions = null)
        {
            this.allOptionCodes = allOptionCodes.ToArray();
            this.selectedNoCodes = checkedOptions?.Where(x => x.No).Select(x => x.Value).ToArray() ?? new int[0];
            this.selectedYesCodes = checkedOptions?.Where(x => x.Yes).Select(x => x.Value).ToArray() ?? new int[0];
        }

        private readonly int[] selectedYesCodes;
        private readonly int[] selectedNoCodes;
        private readonly int[] allOptionCodes;


        public int[] All => this.allOptionCodes;

        public int[] Yes => this.selectedYesCodes;

        public int[] No => this.selectedNoCodes;

        public int[] Missing => this.allOptionCodes.Except(this.selectedYesCodes).Except(this.selectedNoCodes).ToArray();

        public bool? this[int code]
        {
            get
            {
                if (!this.allOptionCodes.Contains(code))
                {
                    throw new IndexOutOfRangeException("Option with code {code} is absent");
                }
                if (this.selectedNoCodes.Contains(code))
                {
                    return false;
                }
                if (this.selectedYesCodes.Contains(code))
                {
                    return true;
                }
                return null;
            }
        }
    }
    // old type with decimal arrays for ExpressionState
    [Obsolete("Since v 5.21. July 1 2017")]
    public class YesNoAnswers
    {
        public YesNoAnswers(YesNoAnswers yesNoAnswers)
        {
            this.allOptionCodes = yesNoAnswers.allOptionCodes;
            this.selectedNoCodes = yesNoAnswers.selectedNoCodes;
            this.selectedYesCodes = yesNoAnswers.selectedYesCodes;
        }

        public YesNoAnswers(decimal[] allOptionCodes, YesNoAnswersOnly yesNoAnswersOnly = null)
        {
            this.allOptionCodes = allOptionCodes;
            this.selectedNoCodes = yesNoAnswersOnly?.No ?? new decimal[0];
            this.selectedYesCodes = yesNoAnswersOnly?.Yes ??  new decimal[0];
        }

        private decimal[] selectedYesCodes;
        private decimal[] selectedNoCodes;
        private readonly decimal[] allOptionCodes;


        public decimal[] All => this.allOptionCodes;

        public decimal[] Yes => this.selectedYesCodes;

        public decimal[] No => this.selectedNoCodes;

        public decimal[] Missing => this.allOptionCodes.Except(this.selectedYesCodes).Except(this.selectedNoCodes).ToArray();

        public bool? this[int code]
        {
            get
            {
                if (!this.allOptionCodes.Contains(code))
                {
                    throw new IndexOutOfRangeException("Option with code {code} is absent");
                }
                if (this.selectedNoCodes.Contains(code))
                {
                    return false;
                }
                if (this.selectedYesCodes.Contains(code))
                {
                    return true;
                }
                return null;
            }
        }
    }

    // old type with decimal arrays to communicate between ExpressionState and Interview
    [Obsolete("Since v 5.21. July 1 2017")]
    public class YesNoAnswersOnly
    {
        public YesNoAnswersOnly(decimal[] yes, decimal[] no)
        {
            this.Yes = yes;
            this.No = no;
        }

        public decimal[] Yes { get; }

        public decimal[] No { get; }
    }
}