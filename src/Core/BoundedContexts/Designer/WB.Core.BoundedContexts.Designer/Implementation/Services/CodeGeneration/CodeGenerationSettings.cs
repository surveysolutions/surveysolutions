using System;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    internal class CodeGenerationSettings
    {
        public CodeGenerationSettings(string[] additionInterfaces, string[] namespaces, bool isLookupTablesFeatureSupported, Func<QuestionnaireExpressionStateModel, string> expressionStateBodyGenerator)
        {
            this.AdditionInterfaces = additionInterfaces;
            this.Namespaces = namespaces;
            this.IsLookupTablesFeatureSupported = isLookupTablesFeatureSupported;
            this.ExpressionStateBodyGenerator = expressionStateBodyGenerator;
        }

        public string[] AdditionInterfaces { get; }

        public string[] Namespaces { get; }

        public bool IsLookupTablesFeatureSupported { get; }

        public Func<QuestionnaireExpressionStateModel, string> ExpressionStateBodyGenerator { get; }
    }
}
