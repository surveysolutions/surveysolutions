using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2.Models
{
    public class OptionsFilterMethodModel : ConditionMethodModel
    {
        public OptionsFilterMethodModel(ExpressionLocation location, string className, string methodName, string expression, string variable)
            : base(location, className, methodName, expression, true, variable, "bool")
        {
        }
    }
}
