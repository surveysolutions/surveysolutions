using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.CodeGenerationV2.Models;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2
{
    public class CodeGenerationModelsFactory : ICodeGenerationModelsFactory
    {
        private readonly IMacrosSubstitutionService macrosSubstitutionService;
        private readonly ILookupTableService lookupTableService;
        private readonly IQuestionTypeToCSharpTypeMapper questionTypeMapper;

        public CodeGenerationModelsFactory(
            IMacrosSubstitutionService macrosSubstitutionService, 
            ILookupTableService lookupTableService, 
            IQuestionTypeToCSharpTypeMapper questionTypeMapper)
        {
            this.macrosSubstitutionService = macrosSubstitutionService;
            this.lookupTableService = lookupTableService;
            this.questionTypeMapper = questionTypeMapper;
        }

        public ExpressionStorageModel CreateModel(ReadOnlyQuestionnaireDocument questionnaire)
        {
            var codeGenerationModel = new ExpressionStorageModel
            {
                Id = questionnaire.PublicKey,
                ClassName = $"{CodeGeneratorV2.InterviewExpressionStatePrefix}_{Guid.NewGuid().FormatGuid()}",
                IdMap = this.CreateIdMap(questionnaire)
            };

            Dictionary<RosterScope, string> levelClassNames = new Dictionary<RosterScope, string>();

            Dictionary<RosterScope, Group[]> rosterScopes = questionnaire.Find<Group>()
                .GroupBy(questionnaire.GetRosterScope)
                .ToDictionary(x => x.Key, x => x.ToArray());

            foreach (var rosterScopePairs in rosterScopes)
            {
                var rosterScope = rosterScopePairs.Key;
                var rosters = rosterScopePairs.Value;
                var firstRosterInScope = rosters.FirstOrDefault(x => x.IsRoster);

                string variable;
                string levelClassName;

                if (firstRosterInScope == null)
                {
                    variable = CodeGeneratorV2.QuestionnaireIdName;
                    levelClassName = CodeGeneratorV2.QuestionnaireLevel;
                }
                else
                {
                    variable = questionnaire.GetVariable(firstRosterInScope);
                    levelClassName = CodeGeneratorV2.LevelPrefix + variable;
                }

                var levelModel = new LevelModel(variable, rosterScope, levelClassName);

                levelClassNames.Add(rosterScope, levelClassName);

                codeGenerationModel.Levels.Add(levelModel);
            }
            
            foreach (var level in codeGenerationModel.Levels)
            {
                level.Init(questionnaire, levelClassNames, questionTypeMapper);
            }

            codeGenerationModel.ExpressionMethodModel.AddRange(this.CreateMethodModels(questionnaire, levelClassNames));
            codeGenerationModel.LinkedFilterMethodModel.AddRange(this.CreateLinkedFilterModels(questionnaire, levelClassNames));
            codeGenerationModel.CategoricalOptionsFilterModel.AddRange(this.CreateCategoricalOptionsFilterModels(questionnaire, levelClassNames));
            codeGenerationModel.VariableMethodModel.AddRange(this.CreateVariableMethodModel(questionnaire, levelClassNames));

            return codeGenerationModel;
        }

        public string GenerateQuestionTypeName(IQuestion question, ReadOnlyQuestionnaireDocument questionnaire)
        {
            return questionTypeMapper.GetQuestionType(question, questionnaire);
        }

        private Dictionary<Guid, string> CreateIdMap(ReadOnlyQuestionnaireDocument questionnaire)
        {
            var map = questionnaire.Find<IQuestion>().ToDictionary(x => x.PublicKey, questionnaire.GetVariable);
            questionnaire.Find<StaticText>().ForEach(x => map.Add(x.PublicKey, questionnaire.GetVariable(x)));
            questionnaire.Find<IGroup>().Where(x=> x.IsRoster).ForEach(x => map.Add(x.PublicKey, questionnaire.GetVariable(x)));
            questionnaire.Find<IVariable>().ForEach(x => map.Add(x.PublicKey, questionnaire.GetVariable(x)));
            questionnaire.Find<IGroup>().Where(x => !x.IsRoster).ForEach(x => map.Add(x.PublicKey, questionnaire.GetVariable(x)));
            map.Add(questionnaire.PublicKey, CodeGeneratorV2.QuestionnaireIdName);
            return map;
        }

        public IEnumerable<LinkedFilterMethodModel> CreateLinkedFilterModels(ReadOnlyQuestionnaireDocument questionnaire, Dictionary<RosterScope, string> levelClassNames)
        {
            var linkedWithFilter = questionnaire
                .Find<IQuestion>()
                .Where(x => !string.IsNullOrWhiteSpace(this.macrosSubstitutionService.InlineMacros(x.LinkedFilterExpression, questionnaire.Macros.Values)));

            foreach (var question in linkedWithFilter)
            {
                if (!question.LinkedToRosterId.HasValue && !question.LinkedToQuestionId.HasValue) continue;

                var levelClassName = levelClassNames[questionnaire.GetRosterScope(question.PublicKey)];

                var sourceLevelClassName = levelClassNames[question.LinkedToQuestionId.HasValue 
                    ? questionnaire.GetRosterScope(question.LinkedToQuestionId.Value) 
                    : questionnaire.GetRosterScope(questionnaire.Find<IGroup>(question.LinkedToRosterId!.Value) 
                                                   ?? throw new InvalidOperationException("Referenced roster was not found."))];

                var linkedFilterExpression = this.macrosSubstitutionService.InlineMacros(question.LinkedFilterExpression, questionnaire.Macros.Values);

                var variableName = questionnaire.GetVariable(question);
                yield return
                    new LinkedFilterMethodModel(
                        ExpressionLocation.LinkedQuestionFilter(question.PublicKey),
                        sourceLevelClassName,
                        $"{CodeGeneratorV2.LinkedFilterPrefix}{variableName}",
                        linkedFilterExpression,
                        variableName,
                        levelClassName);
            }
        }


        private IEnumerable<ConditionMethodModel> CreateVariableMethodModel(ReadOnlyQuestionnaireDocument questionnaire, Dictionary<RosterScope, string> levelClassNames)
        {
            var variables = questionnaire
                .Find<IVariable>()
                .Where(x => !string.IsNullOrWhiteSpace(this.macrosSubstitutionService.InlineMacros(x.Expression, questionnaire.Macros.Values)));

            foreach (var variable in variables)
            {
                var variableName = questionnaire.GetVariable(variable);

                yield return
                    new ConditionMethodModel(
                        ExpressionLocation.Variable(variable.PublicKey),
                        levelClassNames[questionnaire.GetRosterScope(variable)],
                        $"{CodeGeneratorV2.VariablePrefix}{variableName}",
                        macrosSubstitutionService.InlineMacros(variable.Expression, questionnaire.Macros.Values),
                        false,
                        variableName,
                        questionTypeMapper.GetVariableType(variable.Type))
                    {
                        UseObjectBoxing = true,
                        ExplicitlyCastToType = true
                    };
            }
        }

        public IEnumerable<OptionsFilterMethodModel> CreateCategoricalOptionsFilterModels(ReadOnlyQuestionnaireDocument questionnaire, Dictionary<RosterScope, string> levelClassNames)
        {
            var questionsWithFilter = questionnaire
                .Find<IQuestion>()
                .Where(x => !string.IsNullOrWhiteSpace(this.macrosSubstitutionService.InlineMacros(x.Properties?.OptionsFilterExpression, questionnaire.Macros.Values)));

            foreach (var question in questionsWithFilter)
            {
                var variable = questionnaire.GetVariable(question);
                var methodName = $"{CodeGeneratorV2.OptionsFilterPrefix}{variable}";
                var expression = this.macrosSubstitutionService.InlineMacros(question.Properties?.OptionsFilterExpression, questionnaire.Macros.Values);
                var levelClassName = levelClassNames[questionnaire.GetRosterScope(question)];

                yield return new OptionsFilterMethodModel(
                    ExpressionLocation.CategoricalQuestionFilter(question.PublicKey),
                    levelClassName,
                    methodName,
                    expression,
                    variable);
            }
        }

        public IEnumerable<ConditionMethodModel> CreateMethodModels(ReadOnlyQuestionnaireDocument questionnaire, Dictionary<RosterScope, string> levelClassNames)
        {
            foreach (var question in questionnaire.Find<IQuestion>())
            {
                var conditionExpression = this.macrosSubstitutionService.InlineMacros(question.ConditionExpression, questionnaire.Macros.Values);
                var variable = questionnaire.GetVariable(question);
                var className = levelClassNames[questionnaire.GetRosterScope(question)];

                if (!string.IsNullOrWhiteSpace(conditionExpression))
                {
                    yield return new ConditionMethodModel(
                        ExpressionLocation.QuestionCondition(question.PublicKey),
                        className,
                        CodeGeneratorV2.EnablementPrefix + variable,
                        conditionExpression,
                        false,
                        variable);
                }

                for (int index = 0; index < question.ValidationConditions.Count; index++)
                {
                    var validation = question.ValidationConditions[index];
                    var validationExpression = this.macrosSubstitutionService.InlineMacros(validation.Expression, questionnaire.Macros.Values);

                    if (!string.IsNullOrWhiteSpace(validationExpression))
                    {
                        yield return new ConditionMethodModel(
                            ExpressionLocation.QuestionValidation(question.PublicKey, index),
                            className,
                            $"{CodeGeneratorV2.ValidationPrefix}{variable}__{index.ToString(CultureInfo.InvariantCulture)}",
                            validationExpression,
                            true,
                            variable);
                    }
                }
            }

            foreach (var staticText in questionnaire.Find<StaticText>())
            {
                var className = levelClassNames[questionnaire.GetRosterScope(staticText)];
                var conditionExpression = this.macrosSubstitutionService.InlineMacros(staticText.ConditionExpression, questionnaire.Macros.Values);
                var formattedId = CodeGeneratorV2.StaticText + staticText.PublicKey.FormatGuid();
                if (!string.IsNullOrWhiteSpace(conditionExpression))
                {
                    yield return new ConditionMethodModel(
                        ExpressionLocation.StaticTextCondition(staticText.PublicKey),
                        className,
                        CodeGeneratorV2.EnablementPrefix + formattedId,
                        conditionExpression,
                        false,
                        formattedId);
                }

                for (int index = 0; index < staticText.ValidationConditions.Count; index++)
                {
                    var validation = staticText.ValidationConditions[index];
                    var validationExpression = this.macrosSubstitutionService.InlineMacros(validation.Expression, questionnaire.Macros.Values);
                    yield return new ConditionMethodModel(
                        ExpressionLocation.StaticTextValidation(staticText.PublicKey, index),
                        className,
                        $"{CodeGeneratorV2.ValidationPrefix}{formattedId}__{index.ToString(CultureInfo.InvariantCulture)}",
                        validationExpression,
                        false,
                        formattedId);
                }
            }

            foreach (var group in questionnaire.Find<Group>())
            {
                var rosterScope = questionnaire.GetRosterScope(@group);
               
                string className = levelClassNames[rosterScope];
                string conditionExpression = this.macrosSubstitutionService.InlineMacros(group.ConditionExpression, questionnaire.Macros.Values);
                string formattedId = questionnaire.GetVariable(group);
                if (!string.IsNullOrWhiteSpace(conditionExpression))
                {
                    yield return new ConditionMethodModel(
                        ExpressionLocation.GroupCondition(group.PublicKey),
                        className,
                        CodeGeneratorV2.EnablementPrefix + formattedId,
                        conditionExpression,
                        false,
                        formattedId);
                }
            }
        }

        public IEnumerable<LookupTableTemplateModel> CreateLookupModels(ReadOnlyQuestionnaireDocument questionnaire)
        {
            foreach (var table in questionnaire.LookupTables)
            {
                var lookupTableData = this.lookupTableService.GetLookupTableContent(questionnaire.PublicKey, table.Key);

                if (lookupTableData == null)
                    throw new InvalidOperationException("Lookup table is empty.");

                var tableName = table.Value.TableName;
                var tableTemplateModel = new LookupTableTemplateModel
                (
                    tableName : tableName,
                    typeName : CodeGeneratorV2.LookupPrefix + tableName.ToPascalCase(),
                    tableNameField : CodeGeneratorV2.PrivateFieldsPrefix + tableName.ToCamelCase(),
                    rows : lookupTableData.Rows,
                    variableNames : lookupTableData.VariableNames
                );
                yield return tableTemplateModel;
            }
        }
    }
}
