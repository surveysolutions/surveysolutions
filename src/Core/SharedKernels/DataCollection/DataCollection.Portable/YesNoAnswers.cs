using System;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection
{
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