using System;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Templates;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    internal class CodeGenerationSettings
    {
        public CodeGenerationSettings(
            string[] additionInterfaces, 
            string[] namespaces, 
            bool isLookupTablesFeatureSupported, 
            Func<QuestionnaireExpressionStateModel, string> expressionStateBodyGenerator,
            Func<LinkedFilterConditionDescriptionModel, string> linkedFilterMethodGenerator = null)
        {
            this.AdditionInterfaces = additionInterfaces;
            this.Namespaces = namespaces;
            this.IsLookupTablesFeatureSupported = isLookupTablesFeatureSupported;
            this.ExpressionStateBodyGenerator = expressionStateBodyGenerator;
            this.LinkedFilterMethodGenerator = linkedFilterMethodGenerator ?? (model => new ExpressionMethodTemplate(model).TransformText());
        }

        public string[] AdditionInterfaces { get; }

        public string[] Namespaces { get; }

        public bool IsLookupTablesFeatureSupported { get; }

        public Func<QuestionnaireExpressionStateModel, string> ExpressionStateBodyGenerator { get; }
        public Func<LinkedFilterConditionDescriptionModel, string> LinkedFilterMethodGenerator { get; }
    }
}
