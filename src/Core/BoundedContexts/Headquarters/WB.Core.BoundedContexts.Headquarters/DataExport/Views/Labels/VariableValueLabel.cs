namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views.Labels
{
    internal class VariableValueLabel
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