namespace WB.Core.SharedKernels.DataCollection.Scenarios
{
    public class AnswerText : ScenarioAnswerCommand
    {
        public AnswerText(string variable, RosterVector rosterVector, string answer) : base(variable, rosterVector)
        {
            Answer = answer;
        }

        public string Answer { get; }
    }
}
