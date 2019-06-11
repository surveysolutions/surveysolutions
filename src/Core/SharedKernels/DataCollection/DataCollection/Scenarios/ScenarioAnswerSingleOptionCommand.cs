namespace WB.Core.SharedKernels.DataCollection.Scenarios
{
    public class ScenarioAnswerSingleOptionCommand : ScenarioAnswerCommand
    {
        public ScenarioAnswerSingleOptionCommand(string variable, RosterVector rosterVector, int selectedValue) : base(variable, rosterVector)
        {
            SelectedValue = selectedValue;
        }

        public int SelectedValue { get;  }
    }
}
