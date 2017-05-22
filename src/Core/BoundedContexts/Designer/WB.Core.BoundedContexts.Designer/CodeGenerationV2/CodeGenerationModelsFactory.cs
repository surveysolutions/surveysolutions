using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.CodeGenerationV2.Models;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2
{
    public class CodeGenerationModelsFactory : ICodeGenerationModelsFactory
    {
        private readonly IMacrosSubstitutionService macrosSubstitutionService;
        private readonly ILookupTableService lookupTableService;

        public CodeGenerationModelsFactory(
            IMacrosSubstitutionService macrosSubstitutionService, 
            ILookupTableService lookupTableService)
        {
            this.macrosSubstitutionService = macrosSubstitutionService;
            this.lookupTableService = lookupTableService;
        }

        public ExpressionStorageModel CreateModel(ReadOnlyQuestionnaireDocument questionnaire)
        {
            var codeGenerationModel = new ExpressionStorageModel
            {
                Id = questionnaire.PublicKey,
                ClassName = $"{CodeGeneratorV2.InterviewExpressionStatePrefix}_{Guid.NewGuid().FormatGuid()}",
                IdMap = this.CreateIdMap(questionnaire)
            };

            Dictionary<RosterScope, Group[]> rosterScopes = questionnaire.Find<Group>()
                .GroupBy(questionnaire.GetRosterScope)
                .ToDictionary(x => x.Key, x => x.ToArray());

            Dictionary<RosterScope, string> levelClassNames = new Dictionary<RosterScope, string>();

            foreach (var rosterScopePairs in rosterScopes)
            {
                var rosterScope = rosterScopePairs.Key;
                var rosters = rosterScopePairs.Value;
                var firstRosterInScope = rosters.FirstOrDefault(x => x.IsRoster);

                var levelModel = new LevelModel
                {
                    RosterScope = rosterScope
                };

                string levelClassName = "";
                if (firstRosterInScope == null)
                {
                    levelClassName = CodeGeneratorV2.QuestionnaireLevel;
                    levelModel.Id = questionnaire.PublicKey;
                    levelModel.Variable = CodeGeneratorV2.QuestionnaireIdName;
                }
                else
                {
                    levelModel.Id = firstRosterInScope.PublicKey;
                    levelModel.Variable = this.GetVariable(firstRosterInScope);
                    levelClassName = CodeGeneratorV2.LevelPrefix + levelModel.Variable;
                }

                levelModel.ClassName = levelClassName;
                levelClassNames.Add(rosterScope, levelClassName);

                codeGenerationModel.AllLevels.Add(levelModel);
            }

            foreach (var question in questionnaire.Find<IQuestion>())
            {
                string varName = this.GetVariable(question);

                var rosterScope = questionnaire.GetRosterScope(question);
                var levelClassName = levelClassNames[rosterScope];
                var questionModel = new QuestionModel
                {
                    Id = question.PublicKey,
                    Variable = varName,
                    ClassName = levelClassName,
                    TypeName = GenerateQuestionTypeName(question, questionnaire),
                    RosterScope = rosterScope
                };

                codeGenerationModel.AllQuestions.Add(questionModel);
            }

            foreach (var staticText in questionnaire.Find<StaticText>())
            {
                string varName = this.GetVariable(staticText);

                var rosterScope = questionnaire.GetRosterScope(staticText);
                var levelClassName = levelClassNames[rosterScope];
                var questionModel = new StaticTextModel
                {
                    Id = staticText.PublicKey,
                    Variable = varName,
                    ClassName = levelClassName,
                    RosterScope = rosterScope
                };

                codeGenerationModel.AllStaticTexts.Add(questionModel);
            }

            foreach (var level in codeGenerationModel.AllLevels)
            {
                foreach (var question in codeGenerationModel.AllQuestions)
                {
                    if (question.RosterScope.IsSameOrParentScopeFor(level.RosterScope))
                    {
                        level.Questions.Add(question);
                    }
                }

                foreach (var rosterScopePairs in rosterScopes)
                {
                    var rosterScope = rosterScopePairs.Key;
                    if (!rosterScope.IsSameOrParentScopeFor(level.RosterScope) )
                    {
                        if (!rosterScope.IsChildScopeFor(level.RosterScope, 1))
                        continue;
                    }

                    var rosters = rosterScopePairs.Value.Where(x => x.IsRoster);
                    foreach (var roster in rosters)
                    {
                        level.Rosters.Add(new RosterModel
                        {
                            Variable = this.GetVariable(roster),
                            Level = codeGenerationModel.AllLevels.First(x => x.RosterScope.Equals(rosterScope))
                        });
                    }
                }
            }

            codeGenerationModel.ExpressionMethodModel.AddRange(this.CreateMethodModels(questionnaire, codeGenerationModel));
            codeGenerationModel.LinkedFilterMethodModel.AddRange(this.CreateLinkedFilterModels(questionnaire, codeGenerationModel));
            codeGenerationModel.CategoricalOptionsFilterModel.AddRange(this.CreateCategoricalOptionsFilterModels(questionnaire, codeGenerationModel));

            return codeGenerationModel;
        }

        private Dictionary<Guid, string> CreateIdMap(ReadOnlyQuestionnaireDocument questionnaire)
        {
            var map = questionnaire.Find<IQuestion>().ToDictionary(x => x.PublicKey, this.GetVariable);
            questionnaire.Find<StaticText>().ForEach(x => map.Add(x.PublicKey, this.GetVariable(x)));
            questionnaire.Find<IGroup>().ForEach(x => map.Add(x.PublicKey, this.GetVariable(x)));
            map.Add(questionnaire.PublicKey, CodeGeneratorV2.QuestionnaireIdName);
            return map;
        }

        private string GetVariable(StaticText staticText)
        {
            return CodeGeneratorV2.StaticText + staticText.PublicKey.FormatGuid();
        }

        private string GetVariable(IQuestion question)
        {
            return !String.IsNullOrEmpty(question.StataExportCaption) ? question.StataExportCaption : "__" + question.PublicKey.FormatGuid();
        }

        private string GetVariable(IGroup group)
        {
            return !String.IsNullOrEmpty(group.VariableName) ? group.VariableName : CodeGeneratorV2.SubSection_ + group.PublicKey.FormatGuid();
        }

        private static string GenerateQuestionTypeName(IQuestion question, ReadOnlyQuestionnaireDocument questionnaire)
        {
            switch (question.QuestionType)
            {
                case QuestionType.Text:
                    return "string";

                case QuestionType.Numeric:
                    return ((question as NumericQuestion)?.IsInteger ?? false) ? "int?" : "double?";

                case QuestionType.QRBarcode:
                    return "string";

                case QuestionType.MultyOption:
                    var multiOtion = question as MultyOptionsQuestion;
                    if (multiOtion != null && multiOtion.YesNoView)
                        return typeof(YesNoAnswers).Name;

                    if (question.LinkedToQuestionId == null && question.LinkedToRosterId == null)
                        return "int[]";

                    if (question.LinkedToQuestionId.HasValue && questionnaire.Find<ITextListQuestion>(question.LinkedToQuestionId.Value) != null)
                    {
                        return "int[]";
                    }
                    return $"{typeof(RosterVector).Name}[]";

                case QuestionType.DateTime:
                    return "DateTime?";

                case QuestionType.SingleOption:
                    if (question.LinkedToQuestionId == null && question.LinkedToRosterId == null) return "int?";

                    if (question.LinkedToQuestionId.HasValue && questionnaire.Find<ITextListQuestion>(question.LinkedToQuestionId.Value) != null)
                    {
                        return "int?";
                    }

                    return typeof(RosterVector).Name;
                case QuestionType.TextList:
                    return $"{typeof(TextListAnswerRow).Name}[]";

                case QuestionType.GpsCoordinates:
                    return typeof(GeoLocation).Name;

                case QuestionType.Multimedia:
                    return "string";

                default:
                    throw new ArgumentException("Unknown question type.");
            }
        }

        public IEnumerable<LinkedFilterMethodModel> CreateLinkedFilterModels(ReadOnlyQuestionnaireDocument questionnaire, ExpressionStorageModel model)
        {
            var linkedWithFilter = questionnaire
                .Find<IQuestion>()
                .Where(x => !string.IsNullOrWhiteSpace(this.macrosSubstitutionService.InlineMacros(x.LinkedFilterExpression, questionnaire.Macros.Values)));

            foreach (var question in linkedWithFilter)
            {
                if (!question.LinkedToRosterId.HasValue && !question.LinkedToQuestionId.HasValue) continue;

                var questionModel = model.GetQuestionById(question.PublicKey);

                string sourceLevelClassName;
                if (question.LinkedToQuestionId.HasValue)
                {
                    var sourceQuestion = model.GetQuestionById(question.LinkedToQuestionId.Value);
                    sourceLevelClassName = sourceQuestion.ClassName;
                }
                else
                {
                    var sourceLevel = model.GetLevelByVariable(this.GetVariable(questionnaire.Find<IGroup>(question.LinkedToRosterId.Value)));
                    sourceLevelClassName = sourceLevel.ClassName;
                }

                var linkedFilterExpression = this.macrosSubstitutionService.InlineMacros(question.LinkedFilterExpression, questionnaire.Macros.Values);
                yield return
                    new LinkedFilterMethodModel(
                        ExpressionLocation.LinkedQuestionFilter(questionModel.Id),
                        sourceLevelClassName,
                        $"{CodeGeneratorV2.LinkedFilterPrefix}{questionModel.Variable}",
                        linkedFilterExpression,
                        questionModel.ClassName);
            }
        }

        public IEnumerable<OptionsFilterMethodModel> CreateCategoricalOptionsFilterModels(ReadOnlyQuestionnaireDocument questionnaire, ExpressionStorageModel model)
        {
            var questionsWithFilter = questionnaire
                .Find<IQuestion>()
                .Where(x => !string.IsNullOrWhiteSpace(this.macrosSubstitutionService.InlineMacros(x.Properties.OptionsFilterExpression, questionnaire.Macros.Values)));

            foreach (var question in questionsWithFilter)
            {
                var questionModel = model.GetQuestionById(question.PublicKey);
                var optionsFilterExpression = this.macrosSubstitutionService.InlineMacros(question.Properties.OptionsFilterExpression, questionnaire.Macros.Values);

                yield return 
                    new OptionsFilterMethodModel(
                        ExpressionLocation.CategoricalQuestionFilter(questionModel.Id),
                        questionModel.ClassName, 
                        $"{CodeGeneratorV2.OptionsFilterPrefix}{questionModel.Variable}", 
                        optionsFilterExpression,
                        questionModel.Variable);
            }
        }

        public IEnumerable<ConditionMethodModel> CreateMethodModels(ReadOnlyQuestionnaireDocument questionnaire, ExpressionStorageModel model)
        {
            foreach (var questionModel in model.AllQuestions)
            {
                var question = questionnaire.Find<IQuestion>(questionModel.Id);
                var conditionExpression = this.macrosSubstitutionService.InlineMacros(question.ConditionExpression, questionnaire.Macros.Values);
                if (!string.IsNullOrWhiteSpace(conditionExpression))
                {
                    yield return new ConditionMethodModel(
                        ExpressionLocation.QuestionCondition(questionModel.Id),
                        questionModel.ClassName,
                        CodeGeneratorV2.EnablementPrefix + questionModel.Variable,
                        question.ConditionExpression,
                        false,
                        questionModel.Variable);
                }

                for (int index = 0; index < question.ValidationConditions.Count; index++)
                {
                    var validation = question.ValidationConditions[index];
                    var validationExpression = this.macrosSubstitutionService.InlineMacros(validation.Expression, questionnaire.Macros.Values);

                    if (!string.IsNullOrWhiteSpace(validationExpression))
                    {
                        yield return new ConditionMethodModel(
                            ExpressionLocation.QuestionValidation(questionModel.Id, index),
                            questionModel.ClassName,
                            $"{CodeGeneratorV2.ValidationPrefix}{questionModel.Variable}__{index.ToString(CultureInfo.InvariantCulture)}",
                            validationExpression,
                            true,
                            questionModel.Variable);
                    }
                }
            }

            foreach (var staticText in questionnaire.Find<StaticText>())
            {
                var className = model.GetClassNameByRosterScope(questionnaire.GetRosterScope(staticText));
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
                if (group.IsRoster)
                {
                    var scopeCoordinate = @group.RosterSizeSource == RosterSizeSourceType.FixedTitles ? @group.PublicKey : @group.RosterSizeQuestionId.Value;
                    rosterScope = new RosterScope(rosterScope.Coordinates.Union(scopeCoordinate.ToEnumerable()));
                }

                string className = model.GetClassNameByRosterScope(rosterScope);
                string conditionExpression = this.macrosSubstitutionService.InlineMacros(group.ConditionExpression, questionnaire.Macros.Values);
                string formattedId = this.GetVariable(group);
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
                var tableName = table.Value.TableName;
                var tableTemplateModel = new LookupTableTemplateModel
                {
                    TableName = tableName,
                    TypeName = CodeGeneratorV2.LookupPrefix + tableName.ToPascalCase(),
                    TableNameField = CodeGeneratorV2.PrivateFieldsPrefix + tableName.ToCamelCase(),
                    Rows = lookupTableData.Rows,
                    VariableNames = lookupTableData.VariableNames
                };
                yield return tableTemplateModel;
            }
        }
    }
}