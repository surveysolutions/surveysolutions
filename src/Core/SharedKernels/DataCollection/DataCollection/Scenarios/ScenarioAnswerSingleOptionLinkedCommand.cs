namespace WB.Core.SharedKernels.DataCollection.Scenarios
{
    public class ScenarioAnswerSingleOptionLinkedCommand : ScenarioAnswerCommand
    {
        public ScenarioAnswerSingleOptionLinkedCommand(string variable, RosterVector rosterVector, RosterVector selectedRosterVector) : base(variable, rosterVector)
        {
            SelectedRosterVector = selectedRosterVector;
        }

        public RosterVector SelectedRosterVector { get; }
    }
}
