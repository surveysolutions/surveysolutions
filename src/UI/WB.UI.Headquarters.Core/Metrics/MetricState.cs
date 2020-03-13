namespace WB.UI.Headquarters.Metrics
{
    public struct MetricState
    {
        public MetricState(string name, string value)
        {
            Name = name;
            Value = value;
        }
        public string Name { get; }
        public string Value { get; }
    }

   
}
