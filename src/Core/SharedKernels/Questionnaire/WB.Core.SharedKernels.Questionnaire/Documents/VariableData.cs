namespace WB.Core.SharedKernels.QuestionnaireEntities
{
    public class VariableData
    {
        protected VariableData()
        {
        }

        public VariableData(VariableType type, string name, string expression, string description)
        {
            this.Type = type;
            this.Name = name;
            this.Expression = expression;
            this.Description = description;
        }

        public VariableType Type { get; set; }
        public string Name { get; set; }
        public string Expression { get; set; }
        public string Description { get; set; }
    }
}