using System;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    internal class CodeGenerationSettings
    {
        public CodeGenerationSettings(string[] additionInterfaces, 
            string[] namespaces,
            bool isLookupTablesFeatureSupported)
        {
            AdditionInterfaces = additionInterfaces;
            Namespaces = namespaces;
            IsLookupTablesFeatureSupported = isLookupTablesFeatureSupported;
        }


        public string[] AdditionInterfaces { get; private set; }

        public string[] Namespaces { get; private set; }

        public bool IsLookupTablesFeatureSupported { get; private set; }

        public Func<QuestionnaireExecutorTemplateModel, string> ExpressionStateBodyGenerator { get; set; }
    }
}
