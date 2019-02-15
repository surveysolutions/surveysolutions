using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Interview;

namespace WB.Services.Export.CsvExport.Implementation.DoFiles
{
    [DebuggerDisplay("{VariableName} {Label}")]
    public class DataExportVariable
    {
        private readonly Dictionary<string, VariableValueLabel> variableValueLabels;

        public DataExportVariable(string variableName, string label, Guid? entityId, VariableValueLabel[] variableValueLabels, ExportValueType valueType)
        {
            this.VariableName = variableName;
            this.Label = label;
            this.EntityId = entityId;

            var uniqueValues = from v in variableValueLabels
                group v by v.Value
                into g
                where g.Count() == 1
                select new
                {
                    Value = g.Key,
                    Label = g.Single()
                };

            this.variableValueLabels = uniqueValues.ToDictionary(x => x.Value, x => x.Label);

            this.ValueType = valueType;
        }

        public DataExportVariable(string variableName, string label):this(variableName,label,null, Array.Empty<VariableValueLabel>(), ExportValueType.Unknown)
        {
        }

        public string VariableName { get; private set; }
        public string Label { get; private set; }
        public Guid? EntityId { get; private set; }
        public VariableValueLabel[] VariableValueLabels => this.variableValueLabels.Values.ToArray();
        public ExportValueType ValueType { set; get; }
    }
}
