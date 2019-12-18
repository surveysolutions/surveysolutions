using System;
using System.Collections.Generic;
using System.Linq;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Interview;

namespace WB.Services.Export.CsvExport.Implementation.DoFiles
{
    public class DataExportLabelValue
    {
        public DataExportLabelValue(string labelName, Guid? entityId, VariableValueLabel[] variableValues)
        {
            this.LabelName = labelName;
            this.EntityId = entityId;

            var uniqueValues = from v in variableValues
                group v by v.Value
                into g
                where g.Count() == 1
                select new
                {
                    Value = g.Key,
                    Label = g.Single()
                };
            this.VariableValues = uniqueValues.ToDictionary(x => x.Value, x => x.Label).Values.ToArray();
            //this.VariableValues = variableValues;

            this.IsReference = false;
        }
        public DataExportLabelValue(string labelName, Guid? entityId)
        {
            this.LabelName = labelName;
            this.EntityId = entityId;
            this.VariableValues = new VariableValueLabel[0];

            this.IsReference = true;
        }

        public string LabelName { get; private set; }
        public Guid? EntityId { get; private set; }
        public VariableValueLabel[] VariableValues { get; }
        public bool IsReference { get; private set; }
    }
}
