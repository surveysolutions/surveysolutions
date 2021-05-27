using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2.Models
{
    public class ConditionMethodModel
    {
        public ConditionMethodModel(ExpressionLocation location, 
            string className, 
            string methodName, 
            string expression, 
            bool generateSelf, 
            string variable, 
            string returnType = "bool")
        {
            this.Location = location;
            this.ClassName = className;
            this.MethodName = methodName;
            this.Expression = expression;
            this.Variable = variable;
            this.GenerateSelf = generateSelf;
            this.ReturnType = returnType;
        }

        public ExpressionLocation Location { get; set; }
        public string ClassName { set; get; }
        public string MethodName { set; get; }
        public string Expression { set; get; }
        public string Variable { set; get; }
        public bool GenerateSelf { set; get; }
        public string ReturnType { get; set; }
        public bool UseObjectBoxing { get; set; } = false;
        
        public bool ExplicitlyCastToType { get; set; } = false;
    }
}
