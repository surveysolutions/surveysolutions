using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.Scenarios
{
    public class Scenario
    {
        public Scenario()
        {
            Steps = new List<IScenarioCommand>();
        }

        public List<IScenarioCommand> Steps { get; set; }
    }
}
