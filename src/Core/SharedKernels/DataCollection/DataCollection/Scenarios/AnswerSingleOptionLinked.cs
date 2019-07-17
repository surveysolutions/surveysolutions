namespace WB.Core.SharedKernels.DataCollection.Scenarios
{
    public class AnswerSingleOptionLinked : ScenarioAnswerCommand
    {
        public AnswerSingleOptionLinked(string variable, RosterVector rosterVector, RosterVector selectedRosterVector) : base(variable, rosterVector)
        {
            SelectedRosterVector = selectedRosterVector;
        }

        public RosterVector SelectedRosterVector { get; }
    }
}
