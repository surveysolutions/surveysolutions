﻿using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.CodeGenerationV2.CodeTemplates;
using WB.Core.BoundedContexts.Designer.CodeGenerationV2.Models;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2
{
    internal class CodeGeneratorV2 : ICodeGeneratorV2
    {
        public const string InterviewExpressionStatePrefix = "ExpressionStorage";
        public const string QuestionnaireIdName = "questionnaire";
        public const string QuestionnaireLevel = "QuestionnaireLevel";
        public const string EnablementPrefix = "IsEnabled__";
        public const string OptionsFilterPrefix = "FilterOption__";
        public const string VariablePrefix = "Variable__";
        
        public const string ValidationPrefix = "IsValid__";
        public const string LinkedFilterPrefix = "FilterForLinkedQuestion__";
        public const string LevelPrefix = "Level_";
        public const string StaticText = "text_";
        public const string LookupPrefix = "Lookup__";
        public const string PrivateFieldsPrefix = "__";
        
        private readonly ICodeGenerationModelsFactory modelsFactory;

        public CodeGeneratorV2(ICodeGenerationModelsFactory modelsFactory)
        {
            this.modelsFactory = modelsFactory;
        }

        public Dictionary<string, string> Generate(QuestionnaireDocument questionnaire, int targetVersion, bool inSingleFile = false)
        {
            var readOnlyQuestionnaireDocument = questionnaire.AsReadOnly();
            ExpressionStorageModel model = this.modelsFactory.CreateModel(readOnlyQuestionnaireDocument);
            model.LookupTables = this.modelsFactory.CreateLookupModels(readOnlyQuestionnaireDocument).ToList();

            var transformText = new InterviewExpressionStorageTemplate(model).TransformText();
            var generatedClasses = new Dictionary<string, string>
            {
                { ExpressionLocation.Questionnaire(questionnaire.PublicKey).Key, transformText }
            };

            var lookupTablesTemplate = new LookupTablesTemplate(model.LookupTables);
            generatedClasses.Add(ExpressionLocation.LookupTables().Key, lookupTablesTemplate.TransformText());

            foreach (ConditionMethodModel variableMethodModel in model.VariableMethodModel)
            {
                var methodTemplate = new ConditionMethodTemplate(variableMethodModel)
                {
                    InSingleFile = inSingleFile
                };
                generatedClasses.Add(variableMethodModel.Location.Key, methodTemplate.TransformText());
            }

            foreach (ConditionMethodModel expressionMethodModel in model.ExpressionMethodModel)
            {
                var methodTemplate = new ConditionMethodTemplate(expressionMethodModel)
                {
                    InSingleFile = inSingleFile
                };
                generatedClasses.Add(expressionMethodModel.Location.Key, methodTemplate.TransformText());
            }

            foreach (OptionsFilterMethodModel categoricalOptionsFilterModel in model.CategoricalOptionsFilterModel)
            {
                var methodTemplate = new OptionsFilterMethodTemplate(categoricalOptionsFilterModel)
                {
                    InSingleFile = inSingleFile
                };
                generatedClasses.Add(categoricalOptionsFilterModel.Location.Key, methodTemplate.TransformText());
            }

            foreach (LinkedFilterMethodModel linkedFilterMethodModel in model.LinkedFilterMethodModel)
            {
                var methodTemplate = new LinkedFilterMethodTemplate(linkedFilterMethodModel)
                {
                    InSingleFile = inSingleFile
                };
                generatedClasses.Add(linkedFilterMethodModel.Location.Key, methodTemplate.TransformText());
            }

            return generatedClasses;
        }
    }
}
