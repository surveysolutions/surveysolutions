namespace WB.Services.Export.Interview.Entities
{
    public class AnsweredYesNoOption
    {
        public decimal OptionValue { get; }

        public bool Yes { get; }

        public AnsweredYesNoOption(decimal optionValue, bool yes)
        {
            this.OptionValue = optionValue;
            this.Yes = yes;
        }

        public override bool Equals(object obj)
        {
            return obj is AnsweredYesNoOption option && this.Equals(option);
        }

        protected bool Equals(AnsweredYesNoOption other)
            => this.OptionValue == other.OptionValue && this.Yes == other.Yes;

        public override int GetHashCode()
        {
            unchecked
            {
                return (this.OptionValue.GetHashCode() * 397) ^ this.Yes.GetHashCode();
            }
        }
    }
}
