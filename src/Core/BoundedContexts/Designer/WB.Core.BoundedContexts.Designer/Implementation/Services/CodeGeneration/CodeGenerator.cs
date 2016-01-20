using System;
using System.Collections.Generic;
using System.Data;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Templates;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V2.Templates;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V5.Templates;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    internal class CodeGenerator : ICodeGenerator
    {
        public const string InterviewExpressionStatePrefix = "InterviewExpressionState";
        public const string PrivateFieldsPrefix = "@__";
        public const string QuestionnaireTypeName = "QuestionnaireTopLevel";
        public const string QuestionnaireScope = "@__questionnaire_scope";
        public const string EnablementPrefix = "IsEnabled_";
        public const string ValidationPrefix = "IsValid_";
        public const string IdSuffix = "_id";
        public const string StateSuffix = "_state";

        private readonly QuestionnaireExpressionStateModelFactory expressionStateModelFactory;

        public CodeGenerator(
            IMacrosSubstitutionService macrosSubstitutionService, 
            IExpressionProcessor expressionProcessor,
            ILookupTableService lookupTableService)
        {
            this.expressionStateModelFactory = new QuestionnaireExpressionStateModelFactory(
                macrosSubstitutionService, 
                expressionProcessor, 
                lookupTableService);
        }
        
        public Dictionary<string, string> Generate(QuestionnaireDocument questionnaire, Version targetVersion)
        {
            CodeGenerationSettings codeGenerationSettings = this.CreateCodeGenerationSettingsBasedOnEngineVersion(targetVersion);

            QuestionnaireExpressionStateModel expressionStateModel = this.expressionStateModelFactory.CreateQuestionnaireExecutorTemplateModel(questionnaire, codeGenerationSettings);

            var transformText = codeGenerationSettings.ExpressionStateBodyGenerator(expressionStateModel);

            var generatedClasses = new Dictionary<string, string>
            {
                { ExpressionLocation.Questionnaire(questionnaire.PublicKey).Key, transformText }
            };

            if (codeGenerationSettings.IsLookupTablesFeatureSupported)
            {
                var lookupTablesTemplate = new LookupTablesTemplateV5(expressionStateModel.LookupTables);
                generatedClasses.Add(ExpressionLocation.LookupTables().Key, lookupTablesTemplate.TransformText());
            }

            foreach (var expressionMethodModel in expressionStateModel.MethodModels)
            {
                var methodTemplate = new ExpressionMethodTemplate(expressionMethodModel.Value);
                generatedClasses.Add(expressionMethodModel.Key, methodTemplate.TransformText());
            }
            
            return generatedClasses;
        }

        private CodeGenerationSettings CreateCodeGenerationSettingsBasedOnEngineVersion(Version version)
        {
            if (version.Major <= 8)
                throw new VersionNotFoundException($"version '{version}' is not found");
            
            if (version.Major == 9)
                return new CodeGenerationSettings(
                    additionInterfaces: new[] { "IInterviewExpressionStateV2" },
                    namespaces: new[]
                    {
                        "WB.Core.SharedKernels.DataCollection.V2",
                        "WB.Core.SharedKernels.DataCollection.V2.CustomFunctions",
                        "WB.Core.SharedKernels.DataCollection.V3.CustomFunctions"
                    },
                    isLookupTablesFeatureSupported: false)
                {
                    ExpressionStateBodyGenerator = expressionStateModel => new InterviewExpressionStateTemplate(expressionStateModel).TransformText()
                };

            if (version.Major == 10)
                return new CodeGenerationSettings(
                    additionInterfaces: new[] { "IInterviewExpressionStateV2", "IInterviewExpressionStateV4" },
                    namespaces: new[]
                    {
                        "WB.Core.SharedKernels.DataCollection.V2",
                        "WB.Core.SharedKernels.DataCollection.V2.CustomFunctions",
                        "WB.Core.SharedKernels.DataCollection.V3.CustomFunctions",
                        "WB.Core.SharedKernels.DataCollection.V4",
                        "WB.Core.SharedKernels.DataCollection.V4.CustomFunctions"
                    },
                    isLookupTablesFeatureSupported: false)
                {
                    ExpressionStateBodyGenerator = expressionStateModel => new InterviewExpressionStateTemplateV2(expressionStateModel).TransformText()
                };
            return new CodeGenerationSettings(
                   additionInterfaces: new[] { "IInterviewExpressionStateV5" },
                   namespaces: new[]
                   {
                        "WB.Core.SharedKernels.DataCollection.V2",
                        "WB.Core.SharedKernels.DataCollection.V2.CustomFunctions",
                        "WB.Core.SharedKernels.DataCollection.V3.CustomFunctions",
                        "WB.Core.SharedKernels.DataCollection.V4",
                        "WB.Core.SharedKernels.DataCollection.V4.CustomFunctions",
                        "WB.Core.SharedKernels.DataCollection.V5",
                        "WB.Core.SharedKernels.DataCollection.V5.CustomFunctions"
                   },
                   isLookupTablesFeatureSupported: true)
            {
                ExpressionStateBodyGenerator = expressionStateModel => new InterviewExpressionStateTemplateV5(expressionStateModel).TransformText()
            };
        }
    }
}