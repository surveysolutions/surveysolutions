namespace WB.Core.SharedKernels.DataCollection.Scenarios
{
    public class ScenarioAnswerMultipleOptionsCommand : ScenarioAnswerCommand
    {
        public ScenarioAnswerMultipleOptionsCommand(string variable, RosterVector rosterVector, int[] selectedValues) : base(variable, rosterVector)
        {
            SelectedValues = selectedValues;
        }

        public int[] SelectedValues { get; }
    }
}
