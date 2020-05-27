using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Main.Core.Documents;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Templates;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V10.Templates;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V5.Templates;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.UI.Designer.Code;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    internal class CodeGenerator : ICodeGenerator
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

        private readonly QuestionnaireExpressionStateModelFactory expressionStateModelFactory;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IOptions<CompilerSettings> settings;

        public CodeGenerator(
            IMacrosSubstitutionService macrosSubstitutionService, 
            IExpressionProcessor expressionProcessor,
            ILookupTableService lookupTableService,
            IFileSystemAccessor fileSystemAccessor,
            IOptions<CompilerSettings> settings)
        {
            this.expressionStateModelFactory = new QuestionnaireExpressionStateModelFactory(
                macrosSubstitutionService, 
                expressionProcessor, 
                lookupTableService);

            this.fileSystemAccessor = fileSystemAccessor;
            this.settings = settings;
        }


        public static string GetQuestionIdName(string? variableName)
        {
            return PrivateFieldsPrefix + variableName + IdSuffix;
        }

        public Dictionary<string, string> Generate(QuestionnaireDocument questionnaire, int targetVersion)
        {
            CodeGenerationSettings codeGenerationSettings = CreateCodeGenerationSettingsBasedOnEngineVersion(targetVersion);

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

            foreach (var categoricalOptionsFilterModel in expressionStateModel.CategoricalOptionsFilterModels)
            {
                var methodTemplate = new OptionsFilterMethodTemplateV10(categoricalOptionsFilterModel.Value);
                generatedClasses.Add(categoricalOptionsFilterModel.Key, methodTemplate.TransformText());
            }

            foreach (var categoricalOptionsFilterModel in expressionStateModel.LinkedFilterModels)
            {
                var methodTemplate = codeGenerationSettings.LinkedFilterMethodGenerator(categoricalOptionsFilterModel.Value);
                generatedClasses.Add(categoricalOptionsFilterModel.Key, methodTemplate);
            }

            this.DumpCodeIfNeeded(generatedClasses);
            
            return generatedClasses;
        }

        private void DumpCodeIfNeeded(Dictionary<string, string> generatedClasses)
        {
            if (!this.settings.Value.EnableDump)
                return;

            if (!this.fileSystemAccessor.IsDirectoryExists(this.settings.Value.DumpFolder))
            {
                this.fileSystemAccessor.CreateDirectory(this.settings.Value.DumpFolder);
            }
            else
            {
                foreach (var filename in this.fileSystemAccessor.GetFilesInDirectory(this.settings.Value.DumpFolder))
                {
                    try
                    {
                        this.fileSystemAccessor.DeleteFile(filename);
                    }
                    catch (Exception)
                    {
                        Debug.WriteLine($"Failed to delete file {filename}.");
                    }
                }
            }

            foreach (var generatedClass in generatedClasses)
            {
                string fileName = this.fileSystemAccessor.MakeStataCompatibleFileName($"{generatedClass.Key}.cs");
                string filePath = this.fileSystemAccessor.CombinePath(this.settings.Value.DumpFolder, fileName);

                this.fileSystemAccessor.WriteAllText(filePath, generatedClass.Value);
            }
        }

        private static CodeGenerationSettings CreateCodeGenerationSettingsBasedOnEngineVersion(int version)
        {
            switch (version)
            {
                case 16:
                case 17:
                case 18:
                case 19:
                    return new CodeGenerationSettings(
                        additionInterfaces: new[] { "IInterviewExpressionStateV10" },
                        namespaces: new[]
                        {
                            "WB.Core.SharedKernels.DataCollection.V2",
                            "WB.Core.SharedKernels.DataCollection.V2.CustomFunctions",
                            "WB.Core.SharedKernels.DataCollection.V3.CustomFunctions",
                            "WB.Core.SharedKernels.DataCollection.V4",
                            "WB.Core.SharedKernels.DataCollection.V4.CustomFunctions",
                            "WB.Core.SharedKernels.DataCollection.V5",
                            "WB.Core.SharedKernels.DataCollection.V5.CustomFunctions",
                            "WB.Core.SharedKernels.DataCollection.V6",
                            "WB.Core.SharedKernels.DataCollection.V7",
                            "WB.Core.SharedKernels.DataCollection.V8",
                            "WB.Core.SharedKernels.DataCollection.V9",
                            "WB.Core.SharedKernels.DataCollection.V10",
                        },
                        isLookupTablesFeatureSupported: true,
                        expressionStateBodyGenerator: expressionStateModel => new InterviewExpressionStateTemplateV10(expressionStateModel).TransformText(),
                        linkedFilterMethodGenerator: model => new LinkedFilterMethodTemplateV10(model).TransformText());
            }

            throw new VersionNotFoundException($"Version '{version}' is not supported.");
        }
    }
}
