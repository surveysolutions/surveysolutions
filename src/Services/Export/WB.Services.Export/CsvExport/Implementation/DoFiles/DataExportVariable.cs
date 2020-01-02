using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Interview;

namespace WB.Services.Export.CsvExport.Implementation.DoFiles
{
    [DebuggerDisplay("{VariableName} {Value.Name}")]
    public class DataExportVariable
    {
        public DataExportVariable(string variableName, string variableLabel, Guid? entityId, VariableValueLabel[] variableValueLabels, ExportValueType valueType)
        {
            this.VariableName = variableName;
            this.VariableLabel = variableLabel;
            this.ValueType = valueType;
            this.Value = new DataExportValue(variableName, entityId, variableValueLabels);
            this.EntityId = entityId;
        }

        public DataExportVariable(string variableName, string variableLabel, Guid? entityId, ExportValueType valueType, DataExportValue value)
        {
            this.VariableName = variableName;
            this.VariableLabel = variableLabel;
            this.Value = value;
            this.EntityId = entityId;
            this.ValueType = valueType;
        }

        public DataExportVariable(string variableName, string label) :
            this(variableName, label, null, Array.Empty<VariableValueLabel>(), ExportValueType.Unknown)
        {
        }

        public string VariableName { get; private set; }
        public string VariableLabel { get; private set; }
        public DataExportValue Value { get; private set; }
        public Guid? EntityId { get; private set; }
        public ExportValueType ValueType { get; private set; }
    }
}
