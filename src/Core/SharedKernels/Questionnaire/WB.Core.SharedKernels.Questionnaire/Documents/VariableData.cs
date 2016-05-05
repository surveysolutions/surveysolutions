namespace WB.Core.SharedKernels.QuestionnaireEntities
{
    public class VariableData
    {
        public VariableData(VariableType type, string name, string body)
        {
            this.Type = type;
            this.Name = name;
            this.Body = body;
        }

        public VariableType Type { get; }
        public string Name { get; }
        public string Body { get; }
    }
}