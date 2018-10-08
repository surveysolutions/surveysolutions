namespace WB.Services.Export.CsvExport.Exporters
{
    public class VariableValueLabel
    {
        public VariableValueLabel(string value, string label)
        {
            this.Value = value;
            this.Label = label;
        }

        public string Value { get; private set; }
        public string Label { get; private set; }
    }
}
