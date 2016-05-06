namespace WB.Core.SharedKernels.QuestionnaireEntities
{
    public class VariableData
    {
        public VariableData(VariableType type, string name, string epression)
        {
            this.Type = type;
            this.Name = name;
            this.Expression = epression;
        }

        public VariableType Type { get; }
        public string Name { get; }
        public string Expression { get; }
    }
}