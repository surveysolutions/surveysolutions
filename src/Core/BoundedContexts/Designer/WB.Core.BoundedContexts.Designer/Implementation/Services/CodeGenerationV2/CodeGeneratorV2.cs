using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGenerationV2.V11;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGenerationV2
{
    internal class CodeGeneratorV2 : ICodeGeneratorV2
    {
        public const string InterviewExpressionStatePrefix = "ExpressionProcessor";
        public const string PrivateFieldsPrefix = "@__";
        public const string QuestionnaireTypeName = "QuestionnaireTopLevel";
        public const string QuestionnaireLevel = "QuestionnaireLevel";
        public const string EnablementPrefix = "IsEnabled__";
        public const string OptionsFilterPrefix = "FilterOption__";
        public const string ValidationPrefix = "IsValid__";
        public const string VariablePrefix = "GetVariable__";
        public const string IdSuffix = "__id";
        public const string StateSuffix = "__state";
        public const string LookupPrefix = "@Lookup__";
        public const string LinkedFilterPrefix = "FilterForLinkedQuestion__";
        public const string LevelPrefix = "Level_";

        private readonly ICodeGenerationModelsFactory modelsFactory;

        public CodeGeneratorV2(ICodeGenerationModelsFactory modelsFactory)
        {
            this.modelsFactory = modelsFactory;
        }

        public Dictionary<string, string> Generate(QuestionnaireDocument questionnaire, int targetVersion)
        {
            var readOnlyQuestionnaireDocument = questionnaire.AsReadOnly();
            CodeGenerationModel model = modelsFactory.CreateModel(readOnlyQuestionnaireDocument);
            var transformText = new InterviewExpressionProcessorTemplateV11(model).TransformText();
            var generatedClasses = new Dictionary<string, string>
            {
                { ExpressionLocation.Questionnaire(questionnaire.PublicKey).Key, transformText }
            };

            foreach (ConditionMethodModel expressionMethodModel in modelsFactory.CreateMethodModels(readOnlyQuestionnaireDocument, model))
            {
                var methodTemplate = new ConditionMethodTemplateV11(expressionMethodModel);
                generatedClasses.Add(expressionMethodModel.Location.Key, methodTemplate.TransformText());
            }

            foreach (var categoricalOptionsFilterModel in modelsFactory.CreateCategoricalOptionsFilterModels(readOnlyQuestionnaireDocument, model))
            {
                var methodTemplate = new OptionsFilterMethodTemplateV11(categoricalOptionsFilterModel);
                generatedClasses.Add(categoricalOptionsFilterModel.Location.Key, methodTemplate.TransformText());
            }

            foreach (LinkedFilterMethodModel categoricalOptionsFilterModel in modelsFactory.CreateLinkedFilterModels(readOnlyQuestionnaireDocument, model))
            {
                var methodTemplate = new LinkedFilterMethodTemplateV11(categoricalOptionsFilterModel);
                generatedClasses.Add(categoricalOptionsFilterModel.Location.Key, methodTemplate.TransformText());
            }

            return generatedClasses;
        }
    }
}
