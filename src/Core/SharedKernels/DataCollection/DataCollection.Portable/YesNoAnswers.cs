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

        public override bool Equals(object obj)
        {
            var targetAnswer = (CheckedYesNoAnswerOption) obj;
            if (targetAnswer == null) return false;

            return this.Value == targetAnswer.Value && this.Yes == targetAnswer.Yes && this.No == targetAnswer.No;
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
