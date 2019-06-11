using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Core.SharedKernels.DataCollection.Scenarios
{
    public class ScenarioAnswerYesNoCommand : ScenarioAnswerCommand
    {
        public ScenarioAnswerYesNoCommand(string variable, RosterVector rosterVector, List<AnsweredYesNoOption> answeredOptions) : base(variable, rosterVector)
        {
            AnsweredOptions = answeredOptions;
        }

        public List<AnsweredYesNoOption> AnsweredOptions { get; }
    }
}
