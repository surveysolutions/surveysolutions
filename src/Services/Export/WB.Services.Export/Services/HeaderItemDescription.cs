using System;
using System.Collections.Generic;
using System.Linq;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Interview;

namespace WB.Services.Export.Services
{
    public class HeaderItemDescription
    {
        public HeaderItemDescription(string label, ExportValueType valueType, VariableValueLabel[] variableValueLabels = null)
        {
            this.ValueType = valueType;
            this.Label = label;
            this.variableValueLabels = (variableValueLabels ?? Array.Empty<VariableValueLabel>()).ToDictionary(x => x.Value, x => x);
        }

        public string Label { get; set; }
        public ExportValueType ValueType { set; get; }

        private readonly Dictionary<string, VariableValueLabel> variableValueLabels;

        public VariableValueLabel[] VariableValueLabels => this.variableValueLabels.Values.ToArray();
    }
}
