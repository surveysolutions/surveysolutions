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

        protected bool Equals(VariableValueLabel other)
        {
            return string.Equals(Value, other.Value) && string.Equals(Label, other.Label);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((VariableValueLabel) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Value != null ? Value.GetHashCode() : 0) * 397) ^ (Label != null ? Label.GetHashCode() : 0);
            }
        }
    }
}
