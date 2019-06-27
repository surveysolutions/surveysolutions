namespace WB.Core.SharedKernels.DataCollection.Scenarios
{
    public abstract class ScenarioAnswerCommand : IScenarioCommand
    {
        protected ScenarioAnswerCommand(string variable, RosterVector rosterVector)
        {
            Variable = variable;
            this.RosterVector = rosterVector ?? RosterVector.Empty;
        }

        public string Variable { get; }

        public RosterVector RosterVector { get; }
    }
}
