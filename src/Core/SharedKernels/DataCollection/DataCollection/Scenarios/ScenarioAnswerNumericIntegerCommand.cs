namespace WB.Core.SharedKernels.DataCollection.Scenarios
{
    public class ScenarioAnswerNumericIntegerCommand : ScenarioAnswerCommand
    {
        public ScenarioAnswerNumericIntegerCommand(string variable, RosterVector rosterVector, int answer) : base(variable, rosterVector)
        {
            Answer = answer;
        }

        public int Answer { get; }
    }
}
