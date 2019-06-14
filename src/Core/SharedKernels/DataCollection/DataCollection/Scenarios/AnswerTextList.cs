using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.Scenarios
{
    public class AnswerTextList : ScenarioAnswerCommand
    {
        public AnswerTextList(string variable, RosterVector rosterVector, List<TextListAnswer> answers) : base(variable, rosterVector)
        {
            Answers = answers;
        }

        public List<TextListAnswer> Answers { get; }
    }
}
