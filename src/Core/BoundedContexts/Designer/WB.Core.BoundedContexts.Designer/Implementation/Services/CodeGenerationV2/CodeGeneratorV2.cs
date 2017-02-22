using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGenerationV2.V11;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGenerationV2
{
    internal class CodeGeneratorV2 : ICodeGeneratorV2
    {
        public const string InterviewExpressionStatePrefix = "InterviewExpressionState";
        public const string PrivateFieldsPrefix = "@__";
        public const string QuestionnaireTypeName = "QuestionnaireTopLevel";
        public const string QuestionnaireScope = "@__questionnaire_scope";
        public const string EnablementPrefix = "IsEnabled__";
        public const string OptionsFilterPrefix = "FilterOption__";
        public const string ValidationPrefix = "IsValid__";
        public const string VariablePrefix = "GetVariable__";
        public const string IdSuffix = "__id";
        public const string StateSuffix = "__state";
        public const string LookupPrefix = "@Lookup__";

        private readonly ICodeGenerationModelsFactory modelsFactory;

        public CodeGeneratorV2(ICodeGenerationModelsFactory modelsFactory)
        {
            this.modelsFactory = modelsFactory;
        }

        public Dictionary<string, string> Generate(QuestionnaireDocument questionnaire, int targetVersion)
        {
            CodeGenerationModel model = modelsFactory.CreateModel(questionnaire.AsReadOnly());
            var transformText = new InterviewExpressionProcessorV11(model).TransformText();
            return new Dictionary<string, string>
            {
                 { ExpressionLocation.Questionnaire(questionnaire.PublicKey).Key, transformText }
            };
        }
    }
}
