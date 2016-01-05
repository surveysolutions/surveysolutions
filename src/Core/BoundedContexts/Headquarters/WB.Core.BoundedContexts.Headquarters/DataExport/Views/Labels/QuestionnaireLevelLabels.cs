using System.Collections.Generic;
using System.Linq;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views.Labels
{
    internal class QuestionnaireLevelLabels
    {
        private readonly Dictionary<string, LabeledVariable> variableLabels;

        public QuestionnaireLevelLabels(string levelName, LabeledVariable[] labeledVariable)
        {
            this.LevelName = levelName;
            this.variableLabels = labeledVariable.ToDictionary(x => x.VariableName, x => x);
        }

        public string LevelName { get; private set; }

        public LabeledVariable[] LabeledVariable => this.variableLabels.Values.ToArray();

        public LabeledVariable this[string variableName] => this.variableLabels[variableName];

        public bool ContainsVariable(string variableName)
        {
            return this.variableLabels.ContainsKey(variableName);
        }
    }
}