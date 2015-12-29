using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
        private readonly QuestionnaireExecutorTemplateModelFactory executorTemplateModelFactory;

        public CodeGenerator(
            IMacrosSubstitutionService macrosSubstitutionService, 
            IExpressionProcessor expressionProcessor,
            ILookupTableService lookupTableService)
        {
            executorTemplateModelFactory = new QuestionnaireExecutorTemplateModelFactory(
                macrosSubstitutionService, 
                expressionProcessor, 
                lookupTableService);
        }

        private static string GenerateExpressionStateBody(QuestionnaireExecutorTemplateModel questionnaireTemplateStructure, Version targetVersion)
        {
            if (targetVersion.Major < 10)
            {
                return new InterviewExpressionStateTemplate(questionnaireTemplateStructure).TransformText();
            }
            else if (targetVersion.Major < 11)
            {
                return new InterviewExpressionStateTemplateV2(questionnaireTemplateStructure).TransformText();
            }
            else
            {
                return new InterviewExpressionStateTemplateV5(questionnaireTemplateStructure).TransformText();
            }
        }

        public Dictionary<string, string> Generate(QuestionnaireDocument questionnaire, Version targetVersion)
        {
            CodeGenerationSettings codeGenerationSettings = CreateCodeGenerationSettingsBasedOnEngineVersion(targetVersion);

            QuestionnaireExecutorTemplateModel questionnaireTemplateStructure = this.executorTemplateModelFactory.CreateQuestionnaireExecutorTemplateModel(questionnaire, codeGenerationSettings, false);

            var transformText = GenerateExpressionStateBody(questionnaireTemplateStructure, targetVersion);

            var generatedClasses = new Dictionary<string, string>();

            generatedClasses.Add(ExpressionLocation.Questionnaire(questionnaire.PublicKey).Key, transformText);

            if (codeGenerationSettings.IsLookupTablesFeatureSupported)
            {
                GenerateLookupTableClasses(questionnaireTemplateStructure.LookupTables, generatedClasses);
            }

            //generating partial classes
            GenerateQuestionnaireLevelExpressionClasses(questionnaireTemplateStructure, generatedClasses);
            GenerateRostersPartialClasses(questionnaireTemplateStructure, generatedClasses);

            return generatedClasses;
        }

        private void GenerateLookupTableClasses(
            List<LookupTableTemplateModel> lookupTables, 
            Dictionary<string, string> generatedClasses)
        {
            var lookupTablesTemplate = new LookupTablesTemplateV5(lookupTables);
            generatedClasses.Add(ExpressionLocation.LookupTables().Key, lookupTablesTemplate.TransformText());
        }

        private CodeGenerationSettings CreateCodeGenerationSettingsBasedOnEngineVersion(Version version)
        {
            if (version.Major <= 8)
                throw new VersionNotFoundException(string.Format("version '{0}' is not found", version));
            
            if (version.Major == 9)
                return new CodeGenerationSettings(
                    abstractConditionalLevelClassName: "AbstractConditionalLevelInstanceV3",
                    additionInterfaces: new[] { "IInterviewExpressionStateV2" },
                    namespaces: new[]
                    {
                        "WB.Core.SharedKernels.DataCollection.V2",
                        "WB.Core.SharedKernels.DataCollection.V2.CustomFunctions",
                        "WB.Core.SharedKernels.DataCollection.V3.CustomFunctions"
                    },
                    areRosterServiceVariablesPresent: true,
                    isLookupTablesFeatureSupported: false);

            if (version.Major == 10)
                return new CodeGenerationSettings(
                    abstractConditionalLevelClassName: "AbstractConditionalLevelInstanceV4",
                    additionInterfaces: new[] { "IInterviewExpressionStateV2", "IInterviewExpressionStateV4" },
                    namespaces: new[]
                    {
                        "WB.Core.SharedKernels.DataCollection.V2",
                        "WB.Core.SharedKernels.DataCollection.V2.CustomFunctions",
                        "WB.Core.SharedKernels.DataCollection.V3.CustomFunctions",
                        "WB.Core.SharedKernels.DataCollection.V4",
                        "WB.Core.SharedKernels.DataCollection.V4.CustomFunctions"
                    },
                    areRosterServiceVariablesPresent: true,
                    isLookupTablesFeatureSupported: false);
            return new CodeGenerationSettings(
                   abstractConditionalLevelClassName: "AbstractConditionalLevelInstanceV5",
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
                   areRosterServiceVariablesPresent: true,
                   isLookupTablesFeatureSupported: true);
        }

        private static void GenerateRostersPartialClasses(
            QuestionnaireExecutorTemplateModel questionnaireTemplateStructure,
            Dictionary<string, string> generatedClasses)
        {
            foreach (var groupedRosters in questionnaireTemplateStructure.RostersGroupedByScope)
            {
                foreach (QuestionTemplateModel questionTemplateModel in groupedRosters.Value.RostersInScope.SelectMany(roster => roster.Questions))
                {
                    if (!string.IsNullOrWhiteSpace(questionTemplateModel.Conditions))
                    {
                        var expressionMethodModel = new ExpressionMethodModel(
                            groupedRosters.Key,
                            questionTemplateModel.GeneratedConditionsMethodName,
                            questionnaireTemplateStructure.Namespaces, 
                            questionTemplateModel.Conditions,
                            false,
                            questionTemplateModel.VariableName);

                        var methodTemplate = new ExpressionMethodTemplate(expressionMethodModel);

                        generatedClasses.Add(ExpressionLocation.QuestionCondition(questionTemplateModel.Id).Key, methodTemplate.TransformText());
                    }

                    if (!string.IsNullOrWhiteSpace(questionTemplateModel.Validations))
                    {
                        var expressionMethodModel = new ExpressionMethodModel(
                          groupedRosters.Key,
                          questionTemplateModel.GeneratedValidationsMethodName,
                          questionnaireTemplateStructure.Namespaces, 
                          questionTemplateModel.Validations,
                          true,
                          questionTemplateModel.VariableName);

                        var methodTemplate =
                            new ExpressionMethodTemplate(expressionMethodModel);

                        generatedClasses.Add(ExpressionLocation.QuestionValidation(questionTemplateModel.Id).Key, methodTemplate.TransformText());
                    }
                }

                foreach (GroupTemplateModel groupTemplateModel in groupedRosters.Value.RostersInScope.SelectMany(roster => roster.Groups))
                {
                    if (!string.IsNullOrWhiteSpace(groupTemplateModel.Conditions))
                    {
                        var expressionMethodModel = new ExpressionMethodModel(
                            groupedRosters.Key,
                            groupTemplateModel.GeneratedConditionsMethodName,
                            questionnaireTemplateStructure.Namespaces, 
                            groupTemplateModel.Conditions,
                            false,
                            groupTemplateModel.VariableName);

                        var methodTemplate = new ExpressionMethodTemplate(expressionMethodModel);

                        generatedClasses.Add(ExpressionLocation.GroupCondition(groupTemplateModel.Id).Key, methodTemplate.TransformText());
                    }
                }

                foreach (RosterTemplateModel rosterTemplateModel in groupedRosters.Value.RostersInScope)
                {
                    if (!string.IsNullOrWhiteSpace(rosterTemplateModel.Conditions))
                    {
                        var expressionMethodModel = new ExpressionMethodModel(
                          groupedRosters.Key,
                          rosterTemplateModel.GeneratedConditionsMethodName,
                          questionnaireTemplateStructure.Namespaces, 
                          rosterTemplateModel.Conditions,
                          false,
                          rosterTemplateModel.VariableName);

                        var methodTemplate = new ExpressionMethodTemplate(expressionMethodModel);

                        generatedClasses.Add(ExpressionLocation.RosterCondition(rosterTemplateModel.Id).Key, methodTemplate.TransformText());
                    }
                }
            }
        }

        private static void GenerateQuestionnaireLevelExpressionClasses(
            QuestionnaireExecutorTemplateModel questionnaireTemplateStructure, 
            Dictionary<string, string> generatedClasses)
        {
            foreach (QuestionTemplateModel questionTemplateModel in questionnaireTemplateStructure.QuestionnaireLevelModel.Questions)
            {
                if (!string.IsNullOrWhiteSpace(questionTemplateModel.Conditions))
                {
                    var expressionMethodModel = new ExpressionMethodModel(
                         questionnaireTemplateStructure.QuestionnaireLevelModel.GeneratedTypeName,
                         questionTemplateModel.GeneratedConditionsMethodName,
                         questionnaireTemplateStructure.Namespaces, 
                         questionTemplateModel.Conditions,
                         false,
                         questionTemplateModel.VariableName);

                    var methodTemplate = new ExpressionMethodTemplate(expressionMethodModel);

                    generatedClasses.Add(ExpressionLocation.QuestionCondition(questionTemplateModel.Id).Key, methodTemplate.TransformText());
                }

                if (!string.IsNullOrWhiteSpace(questionTemplateModel.Validations))
                {
                    var expressionMethodModel = new ExpressionMethodModel(
                        questionnaireTemplateStructure.QuestionnaireLevelModel.GeneratedTypeName,
                        questionTemplateModel.GeneratedValidationsMethodName,
                        questionnaireTemplateStructure.Namespaces, 
                        questionTemplateModel.Validations,
                        true,
                        questionTemplateModel.VariableName);

                    var methodTemplate = new ExpressionMethodTemplate(expressionMethodModel);

                    generatedClasses.Add(ExpressionLocation.QuestionValidation(questionTemplateModel.Id).Key, methodTemplate.TransformText());
                }
            }

            foreach (GroupTemplateModel groupTemplateModel in questionnaireTemplateStructure.QuestionnaireLevelModel.Groups)
            {
                if (!string.IsNullOrWhiteSpace(groupTemplateModel.Conditions))
                {
                    var expressionMethodModel = new ExpressionMethodModel(
                        questionnaireTemplateStructure.QuestionnaireLevelModel.GeneratedTypeName,
                        groupTemplateModel.GeneratedConditionsMethodName,
                        questionnaireTemplateStructure.Namespaces, 
                        groupTemplateModel.Conditions,
                        false,
                        groupTemplateModel.VariableName);

                    var methodTemplate = new ExpressionMethodTemplate(expressionMethodModel);

                    generatedClasses.Add(ExpressionLocation.GroupCondition(groupTemplateModel.Id).Key, methodTemplate.TransformText());
                }
            }
        }
    }
}