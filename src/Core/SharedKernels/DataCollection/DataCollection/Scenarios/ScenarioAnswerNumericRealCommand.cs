namespace WB.Core.SharedKernels.DataCollection.Scenarios
{
    public class ScenarioAnswerNumericRealCommand : ScenarioAnswerCommand
    {
        public ScenarioAnswerNumericRealCommand(string variable, RosterVector rosterVector, double answer) : base(variable, rosterVector)
        {
            Answer = answer;
        }

        public double Answer { get; }
    }
}
