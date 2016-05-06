namespace WB.Core.SharedKernels.QuestionnaireEntities
{
    public class VariableData
    {
        public VariableData(VariableType type, string name, string expression)
        {
            this.Type = type;
            this.Name = name;
            this.Expression = expression;
        }

        public VariableType Type { get; }
        public string Name { get; }
        public string Expression { get; }
    }
}