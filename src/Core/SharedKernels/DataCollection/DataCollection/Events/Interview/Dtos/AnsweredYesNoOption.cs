namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos
{
    public class AnsweredYesNoOption
    {
        public decimal OptionValue { get; private set; }

        public bool Yes { get; private set; }

        public AnsweredYesNoOption(decimal optionValue, bool yes)
        {
            this.OptionValue = optionValue;
            this.Yes = yes;
        }
    }
}