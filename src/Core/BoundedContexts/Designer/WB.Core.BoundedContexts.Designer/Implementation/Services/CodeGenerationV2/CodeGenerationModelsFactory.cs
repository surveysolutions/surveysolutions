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

            return codeGenerationModel;
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
                        return "decimal[]";

                    if (question.LinkedToQuestionId.HasValue && questionnaire.Find<ITextListQuestion>(question.LinkedToQuestionId.Value) != null)
                    {
                        return "decimal[]";
                    }
                    return "decimal[][]";

                case QuestionType.DateTime:
                    return "DateTime?";

                case QuestionType.SingleOption:
                    if (question.LinkedToQuestionId == null && question.LinkedToRosterId == null) return "decimal?";

                    if (question.LinkedToQuestionId.HasValue && questionnaire.Find<ITextListQuestion>(question.LinkedToQuestionId.Value) != null)
                    {
                        return "decimal?";
                    }

                    return "decimal[]";
                case QuestionType.TextList:
                    return "ListAnswerRow[]";

                case QuestionType.GpsCoordinates:
                    return "GeoLocation";

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
        }
    }

    public class ConditionMethodModel
    {
        public ConditionMethodModel(ExpressionLocation location, 
            string className, 
            string methodName, 
            string expression, 
            bool generateSelf, 
            string variableName, 
            string returnType = "bool")
        {
            this.Location = location;
            this.ClassName = className;
            this.MethodName = methodName;
            this.Expression = expression;
            this.VariableName = variableName;
            this.GenerateSelf = generateSelf;
            this.ReturnType = returnType;
        }

        public ExpressionLocation Location { get; set; }
        public string ClassName { set; get; }
        public string MethodName { set; get; }
        public string Expression { set; get; }
        public string VariableName { set; get; }
        public bool GenerateSelf { set; get; }
        public string ReturnType { get; set; }
    }

    public class OptionsFilterMethodModel : ConditionMethodModel
    {
        public OptionsFilterMethodModel(ExpressionLocation location, string className, string methodName, string expression, string variableName)
            : base(location, className, methodName, expression, true, variableName, "bool")
        {
        }
    }

    public class LinkedFilterMethodModel : ConditionMethodModel
    {
        public string SourceLevelClassName { get; }

        public bool IsSourceAndLinkedQuestionOnSameLevel => this.SourceLevelClassName == this.ClassName;

        public LinkedFilterMethodModel(
            ExpressionLocation location, 
            string className, 
            string methodName, 
            string expression, 
            string sourceLevelClassName)
            : base(location, className, methodName, expression, false, string.Empty, "bool")
        {
            this.SourceLevelClassName = sourceLevelClassName;
        }
    }

    public class QuestionModel
    {
        public Guid Id { set; get; }
        public string Variable { set; get; }
        public string ClassName { get; set; }

        public string TypeName { get; set; }
        public RosterScope RosterScope { get; set; }
    }

    public class VariableModel
    {
        public Guid Id { set; get; }
        public string Variable { set; get; }
        public string ClassName { get; set; }
        public string ExpressionMethod { get; set; }
        public string TypeName { get; set; }
        public RosterScope RosterScope { get; set; }
    }

    public class RosterModel
    {
        public string Variable { set; get; }
        public LevelModel Level { get; set; }
    }

    public class LevelModel
    {
        public Guid Id { set; get; }
        public string Variable { set; get; }
        public string ClassName { get; set; }
        public RosterScope RosterScope { get; set; }

        public List<QuestionModel> Questions { get; private set; } = new List<QuestionModel>();
        public List<RosterModel> Rosters { get; private set; } = new List<RosterModel>();
        public List<VariableModel> Variables { get; private set; } = new List<VariableModel>();

    }
}