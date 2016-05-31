namespace WB.Core.SharedKernels.QuestionnaireEntities
{
    public class VariableData
    {
        protected VariableData()
        {
        }

        public VariableData(VariableType type, string name, string expression)
        {
            this.Type = type;
            this.Name = name;
            this.Expression = expression;
        }

        public VariableType Type { get; set; }
        public string Name { get; set; }
        public string Expression { get; set; }
    }
}