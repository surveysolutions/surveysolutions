using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Interview;

namespace WB.Services.Export.CsvExport.Implementation.DoFiles
{
    [DebuggerDisplay("{VariableName} {Label}")]
    internal class DataExportVariable
    {
        private readonly Dictionary<string, VariableValueLabel> variableValueLabels;

        public DataExportVariable(string variableName, string label, Guid? entityId, VariableValueLabel[] variableValueLabels, ExportValueType valueType)
        {
            this.VariableName = variableName;
            this.Label = label;
            this.EntityId = entityId;
            this.variableValueLabels = variableValueLabels.ToDictionary(x => x.Value, x => x);
            this.ValueType = valueType;
        }

        public DataExportVariable(string variableName, string label):this(variableName,label,null, new VariableValueLabel[0], ExportValueType.Unknown)
        {
        }

        public string VariableName { get; private set; }
        public string Label { get; private set; }
        public Guid? EntityId { get; private set; }
        public VariableValueLabel[] VariableValueLabels => this.variableValueLabels.Values.ToArray();
        public ExportValueType ValueType { set; get; }
    }
}
