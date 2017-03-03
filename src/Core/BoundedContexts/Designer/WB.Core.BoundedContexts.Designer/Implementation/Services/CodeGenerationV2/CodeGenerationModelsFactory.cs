using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using static System.String;
using System.Globalization;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGenerationV2
{
    public class CodeGenerationModelsFactory : ICodeGenerationModelsFactory
    {
        private readonly IExpressionProcessor expressionProcessor;
        private readonly IMacrosSubstitutionService macrosSubstitutionService;
        private readonly ILookupTableService lookupTableService;

        public CodeGenerationModelsFactory(
            IExpressionProcessor expressionProcessor,
            IMacrosSubstitutionService macrosSubstitutionService, 
            ILookupTableService lookupTableService)
        {
            this.expressionProcessor = expressionProcessor;
            this.macrosSubstitutionService = macrosSubstitutionService;
            this.lookupTableService = lookupTableService;
        }

        public CodeGenerationModel CreateModel(ReadOnlyQuestionnaireDocument questionnaire)
        {
            var codeGenerationModel = new CodeGenerationModel
            {
                Id = questionnaire.PublicKey,
                ClassName = $"{CodeGeneratorV2.InterviewExpressionStatePrefix}_{Guid.NewGuid().FormatGuid()}",
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
                    levelModel.Variable = GetVariable(firstRosterInScope);
                    levelClassName = CodeGeneratorV2.LevelPrefix + levelModel.Variable;
                }

                levelModel.ClassName = levelClassName;
                levelClassNames.Add(rosterScope, levelClassName);

                codeGenerationModel.AllLevels.Add(levelModel);
            }

            foreach (var question in questionnaire.Find<IQuestion>())
            {
                string varName = GetVariable(question);

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
                string varName = GetVariable(staticText);

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
                    if (!rosterScope.IsSameOrParentScopeFor(level.RosterScope))
                        continue;

                    var rosters = rosterScopePairs.Value.Where(x => x.IsRoster);
                    foreach (var roster in rosters)
                    {
                        level.Rosters.Add(new RosterModel
                        {
                            Variable = GetVariable(roster),
                            Level = codeGenerationModel.AllLevels.First(x => x.RosterScope.Equals(rosterScope))
                        });
                    }
                }
            }

            codeGenerationModel.ExpressionMethodModel.AddRange(CreateMethodModels(questionnaire, codeGenerationModel));
            codeGenerationModel.LinkedFilterMethodModel.AddRange(this.CreateLinkedFilterModels(questionnaire, codeGenerationModel));
            codeGenerationModel.CategoricalOptionsFilterModel.AddRange(this.CreateCategoricalOptionsFilterModels(questionnaire, codeGenerationModel));

            return codeGenerationModel;
        }

        private string GetVariable(StaticText staticText)
        {
            return CodeGeneratorV2.StaticText + staticText.PublicKey.FormatGuid();
        }

        private string GetVariable(IQuestion question)
        {
            return !IsNullOrEmpty(question.StataExportCaption) ? question.StataExportCaption : "__" + question.PublicKey.FormatGuid();
        }

        private string GetVariable(IGroup group)
        {
            return !IsNullOrEmpty(group.VariableName) ? group.VariableName :  "__" + group.PublicKey.FormatGuid();
        }

        private static string GenerateQuestionTypeName(IQuestion question, ReadOnlyQuestionnaireDocument questionnaire)
        {
            switch (question.QuestionType)
            {
                case QuestionType.Text:
                    return "string";

                case QuestionType.Numeric:
                    return ((question as NumericQuestion)?.IsInteger ?? false) ? "long?" : "double?";

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
                    if (question.LinkedToQuestionId == null && question.LinkedToRosterId == null) return "decimal?";

                    if (question.LinkedToQuestionId.HasValue && questionnaire.Find<ITextListQuestion>(question.LinkedToQuestionId.Value) != null)
                    {
                        return "int?";
                    }

                    return typeof(RosterVector).Name;
                case QuestionType.TextList:
                    return $"{typeof(ListAnswerRow).Name}[]";

                case QuestionType.GpsCoordinates:
                    return typeof(GeoLocation).Name;

                case QuestionType.Multimedia:
                    return "string";

                default:
                    throw new ArgumentException("Unknown question type.");
            }
        }

        public IEnumerable<LinkedFilterMethodModel> CreateLinkedFilterModels(ReadOnlyQuestionnaireDocument questionnaire, CodeGenerationModel model)
        {
            var linkedWithFilter = questionnaire
                .Find<IQuestion>()
                .Where(x => !string.IsNullOrWhiteSpace(macrosSubstitutionService.InlineMacros(x.LinkedFilterExpression, questionnaire.Macros.Values)));

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
                    var sourceLevel = model.GetLevelByVariable(GetVariable(questionnaire.Find<IGroup>(question.LinkedToRosterId.Value)));
                    sourceLevelClassName = sourceLevel.ClassName;
                }

                var linkedFilterExpression = macrosSubstitutionService.InlineMacros(question.LinkedFilterExpression, questionnaire.Macros.Values);
                yield return
                    new LinkedFilterMethodModel(
                        ExpressionLocation.LinkedQuestionFilter(questionModel.Id),
                        sourceLevelClassName,
                        $"{CodeGeneratorV2.LinkedFilterPrefix}{questionModel.Variable}",
                        linkedFilterExpression,
                        questionModel.ClassName);
            }
        }

        public IEnumerable<OptionsFilterMethodModel> CreateCategoricalOptionsFilterModels(ReadOnlyQuestionnaireDocument questionnaire, CodeGenerationModel model)
        {
            var questionsWithFilter = questionnaire
                .Find<IQuestion>()
                .Where(x => !string.IsNullOrWhiteSpace(macrosSubstitutionService.InlineMacros(x.Properties.OptionsFilterExpression, questionnaire.Macros.Values)));

            foreach (var question in questionsWithFilter)
            {
                var questionModel = model.GetQuestionById(question.PublicKey);
                var optionsFilterExpression = macrosSubstitutionService.InlineMacros(question.Properties.OptionsFilterExpression, questionnaire.Macros.Values);

                yield return 
                    new OptionsFilterMethodModel(
                        ExpressionLocation.CategoricalQuestionFilter(questionModel.Id),
                        questionModel.ClassName, 
                        $"{CodeGeneratorV2.OptionsFilterPrefix}{questionModel.Variable}", 
                        optionsFilterExpression,
                        questionModel.Variable);
            }
        }

        public IEnumerable<ConditionMethodModel> CreateMethodModels(ReadOnlyQuestionnaireDocument questionnaire, CodeGenerationModel model)
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
            }
        }
    }
}