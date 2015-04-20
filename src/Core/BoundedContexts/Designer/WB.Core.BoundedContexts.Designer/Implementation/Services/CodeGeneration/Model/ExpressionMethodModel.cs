namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class ExpressionMethodModel
    {
        public ExpressionMethodModel(string generatedClassName, string generatedMethodName, string[] namespaces,
            string expressionString)
        {
            GeneratedClassName = generatedClassName;
            GeneratedMethodName = generatedMethodName;
            Namespaces = namespaces;
            ExpressionString = expressionString;
        }

        public string GeneratedClassName { set; get; }
        public string GeneratedMethodName { set; get; }
        public string[] Namespaces { get; set; }
        public string ExpressionString { set; get; }
    }
}
