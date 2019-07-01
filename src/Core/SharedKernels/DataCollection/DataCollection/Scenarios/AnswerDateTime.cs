using System;

namespace WB.Core.SharedKernels.DataCollection.Scenarios
{
    public class AnswerDateTime : ScenarioAnswerCommand
    {
        public AnswerDateTime(string variable, RosterVector rosterVector, DateTime answer) : base(variable, rosterVector)
        {
            Answer = answer;
        }

        public DateTime Answer { get; }
    }
}
