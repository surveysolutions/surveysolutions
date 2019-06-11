namespace WB.Core.SharedKernels.DataCollection.Scenarios
{
    public class ScenarioAnswerTextCommand : ScenarioAnswerCommand
    {
        public ScenarioAnswerTextCommand(string variable, RosterVector rosterVector, string answer) : base(variable, rosterVector)
        {
            Answer = answer;
        }

        public string Answer { get; }
    }
}
