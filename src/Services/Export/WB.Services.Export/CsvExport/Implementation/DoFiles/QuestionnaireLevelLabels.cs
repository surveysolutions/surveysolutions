using System.Collections.Generic;
using System.Linq;

namespace WB.Services.Export.CsvExport.Implementation.DoFiles
{
    internal class QuestionnaireLevelLabels
    {
        private readonly Dictionary<string, DataExportVariable> variableLabels;

        public QuestionnaireLevelLabels(string levelName, DataExportVariable[] labeledVariable)
        {
            this.LevelName = levelName;
            this.variableLabels = labeledVariable.ToDictionary(x => x.VariableName, x => x);
        }

        public string LevelName { get; private set; }

        public DataExportVariable[] LabeledVariable => this.variableLabels.Values.ToArray();

        public DataExportVariable this[string variableName] => this.variableLabels[variableName];

        public bool ContainsVariable(string variableName)
        {
            return this.variableLabels.ContainsKey(variableName);
        }
    }
}
