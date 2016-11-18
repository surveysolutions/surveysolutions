using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Templates;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V10.Templates;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V2.Templates;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V5.Templates;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V6.Templates;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V7.Templates;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V8.Templates;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V9.Templates;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.Infrastructure.FileSystem;

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
        private readonly ICompilerSettings settings;

        public CodeGenerator(
            IMacrosSubstitutionService macrosSubstitutionService, 
            IExpressionProcessor expressionProcessor,
            ILookupTableService lookupTableService,
            IFileSystemAccessor fileSystemAccessor,
            ICompilerSettings settings)
        {
            this.expressionStateModelFactory = new QuestionnaireExpressionStateModelFactory(
                macrosSubstitutionService, 
                expressionProcessor, 
                lookupTableService);

            this.fileSystemAccessor = fileSystemAccessor;
            this.settings = settings;
        }


        public static string GetQuestionIdName(string variableName)
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
            if (!this.settings.EnableDump)
                return;

            if (!this.fileSystemAccessor.IsDirectoryExists(this.settings.DumpFolder))
            {
                this.fileSystemAccessor.CreateDirectory(this.settings.DumpFolder);
            }
            else
            {
                foreach (var filename in this.fileSystemAccessor.GetFilesInDirectory(this.settings.DumpFolder))
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
                string filePath = this.fileSystemAccessor.CombinePath(this.settings.DumpFolder, fileName);

                this.fileSystemAccessor.WriteAllText(filePath, generatedClass.Value);
            }
        }

        private static CodeGenerationSettings CreateCodeGenerationSettingsBasedOnEngineVersion(int version)
        {
            switch (version)
            {
                case 9: 
                    return new CodeGenerationSettings(
                        additionInterfaces: new[] { "IInterviewExpressionStateV2" },
                        namespaces: new[]
                        {
                            "WB.Core.SharedKernels.DataCollection.V2",
                            "WB.Core.SharedKernels.DataCollection.V2.CustomFunctions",
                            "WB.Core.SharedKernels.DataCollection.V3.CustomFunctions"
                        },
                        isLookupTablesFeatureSupported: false,
                        expressionStateBodyGenerator: expressionStateModel => new InterviewExpressionStateTemplate(expressionStateModel).TransformText());

                case 10:
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
                        isLookupTablesFeatureSupported: false,
                        expressionStateBodyGenerator: expressionStateModel => new InterviewExpressionStateTemplateV2(expressionStateModel).TransformText());

                case 11:
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
                        isLookupTablesFeatureSupported: true,
                        expressionStateBodyGenerator: expressionStateModel => new InterviewExpressionStateTemplateV5(expressionStateModel).TransformText());

                case 12:
                    return new CodeGenerationSettings(
                        additionInterfaces: new[] { "IInterviewExpressionStateV6" },
                        namespaces: new[]
                        {
                            "WB.Core.SharedKernels.DataCollection.V2",
                            "WB.Core.SharedKernels.DataCollection.V2.CustomFunctions",
                            "WB.Core.SharedKernels.DataCollection.V3.CustomFunctions",
                            "WB.Core.SharedKernels.DataCollection.V4",
                            "WB.Core.SharedKernels.DataCollection.V4.CustomFunctions",
                            "WB.Core.SharedKernels.DataCollection.V5",
                            "WB.Core.SharedKernels.DataCollection.V5.CustomFunctions",
                            "WB.Core.SharedKernels.DataCollection.V6"
                        },
                        isLookupTablesFeatureSupported: true,
                        expressionStateBodyGenerator: expressionStateModel => new InterviewExpressionStateTemplateV6(expressionStateModel).TransformText());

                case 13:
                    return new CodeGenerationSettings(
                        additionInterfaces: new[] {"IInterviewExpressionStateV7"},
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
                            "WB.Core.SharedKernels.DataCollection.V7"
                        },
                        isLookupTablesFeatureSupported: true,
                        expressionStateBodyGenerator: expressionStateModel => new InterviewExpressionStateTemplateV7(expressionStateModel).TransformText());

                case 14:
                    return new CodeGenerationSettings(
                        additionInterfaces: new[] {"IInterviewExpressionStateV8"},
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
                        },
                        isLookupTablesFeatureSupported: true,
                        expressionStateBodyGenerator: expressionStateModel => new InterviewExpressionStateTemplateV8(expressionStateModel).TransformText());
                case 15:
                    return new CodeGenerationSettings(
                        additionInterfaces: new[] { "IInterviewExpressionStateV9" },
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
                        },
                        isLookupTablesFeatureSupported: true,
                        expressionStateBodyGenerator: expressionStateModel => new InterviewExpressionStateTemplateV9(expressionStateModel).TransformText());
                case 16:
                case 17:
                case 18:
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