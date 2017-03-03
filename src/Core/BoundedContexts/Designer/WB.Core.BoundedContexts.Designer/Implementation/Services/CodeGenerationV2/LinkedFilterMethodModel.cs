using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGenerationV2
{
    public class LinkedFilterMethodModel : ConditionMethodModel
    {
        public string SourceLevelClassName { get; }

        public bool IsSourceAndLinkedQuestionOnSameLevel => this.SourceLevelClassName == this.ClassName;

        public LinkedFilterMethodModel(
            ExpressionLocation location, 
            string className, 
            string methodName, 
            string expression, 
            string sourceLevelClassName)
            : base(location, className, methodName, expression, false, string.Empty, "bool")
        {
            this.SourceLevelClassName = sourceLevelClassName;
        }
    }
}