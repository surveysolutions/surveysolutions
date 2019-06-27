namespace WB.Core.SharedKernels.DataCollection.Scenarios
{
    public class AnswerReal : ScenarioAnswerCommand
    {
        public AnswerReal(string variable, RosterVector rosterVector, double answer) : base(variable, rosterVector)
        {
            Answer = answer;
        }

        public double Answer { get; }
    }
}
