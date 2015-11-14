using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection
{
    public class YesNoAnswers
    {
        private List<decimal> missing = new List<decimal>();
        private decimal[] yes;
        private decimal[] no;

        public YesNoAnswers(decimal[] yes, decimal[] no)
        {
            this.yes = yes;
            this.no = no;
        }

        public decimal[] Yes => this.yes;

        public decimal[] No => this.no;

        public decimal[] Missing
        {
            get { return this.missing.ToArray(); }
        }

        public void SetAnswer(YesNoAnswersOnly yesNoAnswersOnly)
        {
            this.yes = yesNoAnswersOnly.Yes;
            this.no = yesNoAnswersOnly.No;
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