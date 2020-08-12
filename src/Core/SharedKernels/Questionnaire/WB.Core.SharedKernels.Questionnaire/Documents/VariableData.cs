namespace WB.Core.SharedKernels.QuestionnaireEntities
{
    public class VariableData
    {
        public VariableData(VariableType type, string name, string expression, string label, bool doNotExport)
        {
            this.Type = type;
            this.Name = name;
            this.Expression = expression;
            this.Label = label;
            this.DoNotExport = doNotExport;
        }

        public VariableType Type { get; set; }
        public string Name { get; set; }
        public string Expression { get; set; }
        public string Label { get; set; }
        public bool DoNotExport { get; set; }
    }
}
