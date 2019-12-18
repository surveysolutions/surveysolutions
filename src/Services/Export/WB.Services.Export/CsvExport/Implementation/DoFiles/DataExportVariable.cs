using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Interview;

namespace WB.Services.Export.CsvExport.Implementation.DoFiles
{
    [DebuggerDisplay("{VariableName} {Label.LabelName}")]
    public class DataExportVariable
    {
        public DataExportVariable(string variableName, string label, Guid? entityId, VariableValueLabel[] variableValueLabels, ExportValueType valueType)
        {
            this.VariableName = variableName;
            this.ValueType = valueType;
            this.Label = new DataExportLabelValue(label, entityId, variableValueLabels);
            this.EntityId = entityId;
        }

        public DataExportVariable(string variableName, Guid? entityId, DataExportLabelValue label)
        {
            this.VariableName = variableName;
            this.Label = label;
            this.EntityId = entityId;
        }

        public DataExportVariable(string variableName, string label) :
            this(variableName, label, null, Array.Empty<VariableValueLabel>(), ExportValueType.Unknown)
        {
        }

        public string VariableName { get; private set; }
        public DataExportLabelValue Label { get; private set; }
        public Guid? EntityId { get; private set; }
        public ExportValueType ValueType { get; private set; }
    }
}
