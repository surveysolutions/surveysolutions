using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2.Models
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
            string variableName,
            string sourceLevelClassName)
            : base(location, className, methodName, expression, false, variableName, "bool")
        {
            this.SourceLevelClassName = sourceLevelClassName;
        }
    }
}