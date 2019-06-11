namespace WB.Core.SharedKernels.DataCollection.Scenarios
{
    public class ScenarioAnswerMultipleOptionsLinkedCommand : ScenarioAnswerCommand
    {
        public ScenarioAnswerMultipleOptionsLinkedCommand(string variable, RosterVector rosterVector, RosterVector[] selectedRosterVectors) : base(variable, rosterVector)
        {
            SelectedRosterVectors = selectedRosterVectors;
        }

        public RosterVector[] SelectedRosterVectors { get; }
    }
}
