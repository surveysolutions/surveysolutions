namespace WB.Core.SharedKernels.DataCollection.Scenarios
{
    public class AnswerMultipleOptions : ScenarioAnswerCommand
    {
        public AnswerMultipleOptions(string variable, RosterVector rosterVector, int[] selectedValues) : base(variable, rosterVector)
        {
            SelectedValues = selectedValues;
        }

        public int[] SelectedValues { get; }
    }
}
