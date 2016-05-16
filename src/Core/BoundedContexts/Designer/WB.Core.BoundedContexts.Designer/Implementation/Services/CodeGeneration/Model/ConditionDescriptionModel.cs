namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class ConditionDescriptionModel
    {
        public ConditionDescriptionModel(
            string className, 
            string methodName, 
            string[] namespaces,
            string expression, 
            bool generateSelf, 
            string variableName,
            string returnType = "bool")
        {
            this.ClassName = className;
            this.MethodName = methodName;
            this.Namespaces = namespaces;
            this.Expression = expression;
            this.VariableName = variableName;
            this.GenerateSelf = generateSelf;
            this.ReturnType = returnType;
        }

        public string ClassName { set; get; }
        public string MethodName { set; get; }
        public string[] Namespaces { get; set; }
        public string Expression { set; get; }
        public string VariableName { set; get; }
        public bool GenerateSelf { set; get; }
        public string ReturnType { get; set; }
    }
}
