namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class ConditionMethodAndState
    {
        public ConditionMethodAndState(string methodName, string state)
        {
            this.ConditionMethodName = methodName;
            this.StateName = state;
        }
        public string ConditionMethodName { set; get; }
        public string StateName { set; get; }
    }
}