namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class ConditionMethodAndState
    {
        public ConditionMethodAndState(string methodName, string state, bool isAnswerVerifier = false, string? memberName = null, string? type = null)
        {
            this.Type = type;
            this.MemberName = memberName;
            this.ConditionMethodName = methodName;
            this.StateName = state;
            this.IsAnswerVerifier = isAnswerVerifier;
        }

        public string? MemberName { get; }
        public string? Type { get; }
        public string ConditionMethodName { set; get; }
        public string StateName { set; get; }
        public bool IsAnswerVerifier { get; set; }
    }
}
