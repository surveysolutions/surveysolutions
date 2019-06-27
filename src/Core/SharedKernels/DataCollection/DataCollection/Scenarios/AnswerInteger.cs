namespace WB.Core.SharedKernels.DataCollection.Scenarios
{
    public class AnswerInteger : ScenarioAnswerCommand
    {
        public AnswerInteger(string variable, RosterVector rosterVector, int answer) : base(variable, rosterVector)
        {
            Answer = answer;
        }

        public int Answer { get; }
    }
}
