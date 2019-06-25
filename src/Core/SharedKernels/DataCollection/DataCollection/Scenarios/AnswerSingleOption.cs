namespace WB.Core.SharedKernels.DataCollection.Scenarios
{
    public class AnswerSingleOption : ScenarioAnswerCommand
    {
        public AnswerSingleOption(string variable, RosterVector rosterVector, int selectedValue) : base(variable, rosterVector)
        {
            SelectedValue = selectedValue;
        }

        public int SelectedValue { get;  }
    }
}
