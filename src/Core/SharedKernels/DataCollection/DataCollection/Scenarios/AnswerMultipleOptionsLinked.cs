namespace WB.Core.SharedKernels.DataCollection.Scenarios
{
    public class AnswerMultipleOptionsLinked : ScenarioAnswerCommand
    {
        public AnswerMultipleOptionsLinked(string variable, RosterVector rosterVector, RosterVector[] selectedRosterVectors) : base(variable, rosterVector)
        {
            SelectedRosterVectors = selectedRosterVectors;
        }

        public RosterVector[] SelectedRosterVectors { get; }
    }
}
