using System;
using System.Collections.Generic;
using System.Linq;
using WB.Services.Export.CsvExport.Exporters;

namespace WB.Services.Export.Interview
{
    public class DoExportFileHeader
    {
        private readonly Dictionary<string, VariableValueLabel> variableValueLabels;

        public DoExportFileHeader(string title, string description, ExportValueType valueType, VariableValueLabel[] variableValueLabels = null)
        {
            Title = title;
            Description = description;
            this.ValueType = valueType;
            this.variableValueLabels = (variableValueLabels ?? Array.Empty<VariableValueLabel>()).ToDictionary(x => x.Value, x => x);
        }

        public DoExportFileHeader(string title, string description, ExportValueType valueType, bool addCaption) 
            : this(title, description, valueType)
        {
            AddCaption = addCaption;
        }

        public string Title { get; }
        public string Description { get; }
        public bool AddCaption { get; }

        public ExportValueType ValueType { get;}

        public VariableValueLabel[] VariableValueLabels => this.variableValueLabels.Values.ToArray();
    }
}
