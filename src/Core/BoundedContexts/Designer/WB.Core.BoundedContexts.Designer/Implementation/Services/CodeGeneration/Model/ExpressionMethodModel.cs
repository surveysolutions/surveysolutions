namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class ExpressionMethodModel
    {
        public ExpressionMethodModel(
            string generatedClassName, 
            string generatedMethodName, 
            string[] namespaces,
            string expression, 
            bool generateSelf, 
            string variableName)
        {
            this.GeneratedClassName = generatedClassName;
            this.GeneratedMethodName = generatedMethodName;
            this.Namespaces = namespaces;
            this.Expression = expression;
            this.VariableName = variableName;
            this.GenerateSelf = generateSelf;
        }

        public string GeneratedClassName { set; get; }
        public string GeneratedMethodName { set; get; }
        public string[] Namespaces { get; set; }
        public string Expression { set; get; }
        public string VariableName { set; get; }
        public bool GenerateSelf { set; get; }
    }
}
