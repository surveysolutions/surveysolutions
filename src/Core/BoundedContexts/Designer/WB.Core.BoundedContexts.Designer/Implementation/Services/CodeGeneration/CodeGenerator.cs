using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Templates;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.ExpressionProcessor.Services;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class CodeGenerator : ICodeGenerator
    {
        private const string InterviewExpressionStatePrefix = "InterviewExpressionState";

        private IExpressionProcessor ExpressionProcessor
        {
            get { return ServiceLocator.Current.GetInstance<IExpressionProcessor>(); }
        }

        public string Generate(QuestionnaireDocument questionnaire)
        {
            var questionnaireTemplateStructure = CreateQuestionnaireExecutorTemplateModel(questionnaire);
            var template = new InterviewExpressionStateTemplate(questionnaireTemplateStructure);

            return template.TransformText();
        }

        public QuestionnaireExecutorTemplateModel CreateQuestionnaireExecutorTemplateModel(QuestionnaireDocument questionnaire)
        {
            var template = new QuestionnaireExecutorTemplateModel();

            var questionnaireLevelModel = new QuestionnaireLevelTemplateModel(template);
            var generatedClassName = string.Format("{0}_{1}", InterviewExpressionStatePrefix, Guid.NewGuid().FormatGuid());

            Dictionary<string, string> generatedScopesTypeNames;
            List<QuestionTemplateModel> allQuestions;
            List<GroupTemplateModel> allGroups;
            List<RosterTemplateModel> allRosters;

            this.BuildStructures(questionnaire, questionnaireLevelModel, out generatedScopesTypeNames, out allQuestions, out allGroups, out allRosters);
            var rostersGroupedByScope = allRosters.GroupBy(r => r.GeneratedTypeName).ToDictionary(g => g.Key, g => g.ToList());

            var structuralDependencies = questionnaire
                .GetAllGroups()
                .ToDictionary(group => @group.PublicKey, group => @group.Children.Select(x => x.PublicKey).ToList());

            var variableNames = allQuestions.ToDictionary(q => q.VariableName, q => q.Id);
            allRosters.ForEach(r => variableNames.Add(r.VariableName, questionnaire.PublicKey));

            var conditionalDependencies = this.BuildConditionalDependencies(questionnaire, variableNames);

            template.Id = questionnaire.PublicKey;
            template.AllQuestions = allQuestions;
            template.AllGroups = allGroups;
            template.AllRosters = allRosters;
            template.GeneratedClassName = generatedClassName;
            template.GeneratedScopesTypeNames = generatedScopesTypeNames;
            template.RostersGroupedByScope = rostersGroupedByScope;
            template.ConditionalDependencies = conditionalDependencies;
            template.StructuralDependencies = structuralDependencies;
            template.QuestionnaireLevelModel = questionnaireLevelModel;
            template.VariableNames = variableNames;

            return template;
        }

        private void BuildStructures(QuestionnaireDocument questionnaireDoc, QuestionnaireLevelTemplateModel questionnaireLevelModel,
            out Dictionary<string, string> generatedScopesTypeNames,
            out List<QuestionTemplateModel> allQuestions, out List<GroupTemplateModel> allGroups, out List<RosterTemplateModel> allRosters)
        {
            generatedScopesTypeNames = new Dictionary<string, string>();
            allQuestions = new List<QuestionTemplateModel>();
            allGroups = new List<GroupTemplateModel>();
            allRosters = new List<RosterTemplateModel>();

            var rostersToProcess = new Queue<Tuple<IGroup, IRosterScope>>();
            rostersToProcess.Enqueue(new Tuple<IGroup, IRosterScope>(questionnaireDoc, questionnaireLevelModel));

            while (rostersToProcess.Count != 0)
            {
                var rosterScope = rostersToProcess.Dequeue();
                var currentScope = rosterScope.Item2;

                var childrenOfCurrentRoster = new Queue<IComposite>();

                foreach (var childGroup in rosterScope.Item1.Children)
                {
                    childrenOfCurrentRoster.Enqueue(childGroup);
                }

                while (childrenOfCurrentRoster.Count != 0)
                {
                    var child = childrenOfCurrentRoster.Dequeue();

                    var childAsIQuestion = child as IQuestion;
                    if (childAsIQuestion != null)
                    {
                        var varName = !String.IsNullOrEmpty(childAsIQuestion.StataExportCaption)
                            ? childAsIQuestion.StataExportCaption
                            : "__" + childAsIQuestion.PublicKey.FormatGuid();

                        var question = new QuestionTemplateModel()
                        {
                            Id = childAsIQuestion.PublicKey,
                            VariableName = varName,
                            Conditions = childAsIQuestion.ConditionExpression,
                            Validations = childAsIQuestion.ValidationExpression,
                            QuestionType = childAsIQuestion.QuestionType,
                            GeneratedTypeName = this.GenerateQuestionTypeName(childAsIQuestion),
                            GeneratedMemberName = "@__" + varName,
                            GeneratedStateName = "@__" + varName + "_state",
                            GeneratedIdName = "@__" + varName + "_id",
                            GeneratedConditionsMethodName = "IsEnabled_" + varName,
                            GeneratedValidationsMethodName = "IsValid_" + varName,
                            GeneratedMandatoryMethodName = "IsManadatoryValid_" + varName,
                            IsMandatory = childAsIQuestion.Mandatory,
                            RosterScopeName = currentScope.GeneratedRosterScopeName
                        };

                        currentScope.Questions.Add(question);

                        allQuestions.Add(question);

                        continue;
                    }

                    var childAsIGroup = child as IGroup;
                    if (childAsIGroup != null)
                    {
                        if (IsRosterGroup(childAsIGroup))
                        {
                            Guid currentScopeId = childAsIGroup.RosterSizeSource == RosterSizeSourceType.FixedTitles
                                ? childAsIGroup.PublicKey
                                : childAsIGroup.RosterSizeQuestionId.Value;

                            var currentRosterScope = currentScope.GetRosterScope().Select(t => t).ToList();
                            currentRosterScope.Add(currentScopeId);

                            var varName = !String.IsNullOrWhiteSpace(childAsIGroup.VariableName)
                                ? childAsIGroup.VariableName
                                : "__" + childAsIGroup.PublicKey.FormatGuid();

                            var roster = new RosterTemplateModel()
                            {
                                Id = childAsIGroup.PublicKey,
                                Conditions = childAsIGroup.ConditionExpression,
                                VariableName = varName,
                                GeneratedTypeName = this.GenerateTypeNameByScope(currentRosterScope, generatedScopesTypeNames),
                                GeneratedStateName = "@__" + varName + "_state",
                                ParentScope = currentScope,
                                GeneratedIdName = "@__" + varName + "_id",
                                GeneratedConditionsMethodName = "IsEnabled_" + varName,
                                RosterScope = currentRosterScope,
                                GeneratedRosterScopeName = "@__" + varName + "_scope",
                            };

                            rostersToProcess.Enqueue(new Tuple<IGroup, IRosterScope>(childAsIGroup, roster));
                            allRosters.Add(roster);
                            currentScope.Rosters.Add(roster);
                        }
                        else
                        {
                            var varName = childAsIGroup.PublicKey.FormatGuid();
                            var group =
                                new GroupTemplateModel()
                                {
                                    Id = childAsIGroup.PublicKey,
                                    Conditions = childAsIGroup.ConditionExpression,
                                    VariableName = "@__" + varName, //generating variable name by publicKey
                                    GeneratedStateName = "@__" + varName + "_state",
                                    GeneratedIdName = "@__" + varName + "_id",
                                    GeneratedConditionsMethodName = "IsEnabled_" + varName,
                                    RosterScopeName = currentScope.GeneratedRosterScopeName
                                };

                            currentScope.Groups.Add(group);
                            allGroups.Add(group);
                            foreach (var childGroup in childAsIGroup.Children)
                            {
                                childrenOfCurrentRoster.Enqueue(childGroup);
                            }
                        }
                    }
                }
            }

        }

        private string GenerateQuestionTypeName(IQuestion question)
        {
            switch (question.QuestionType)
            {
                case QuestionType.Text:
                    return "string";

                case QuestionType.AutoPropagate:
                    return "long?";

                case QuestionType.Numeric:
                    return (question as NumericQuestion).IsInteger ? "long?" : "decimal?";

                case QuestionType.QRBarcode:
                    return "string";

                case QuestionType.MultyOption:
                    return (question.LinkedToQuestionId == null) ? "decimal[]" : "decimal[][]";

                case QuestionType.DateTime:
                    return "DateTime?";

                case QuestionType.SingleOption:
                    return (question.LinkedToQuestionId == null) ? "decimal?" : "decimal[]";

                case QuestionType.TextList:
                    return "Tuple<decimal, string>[]";

                //TODO: should be fixed with custom type
                case QuestionType.GpsCoordinates:
                    return "decimal?";
                default:
                    throw new ArgumentException("Unknown question type.");
            }
        }

        private Dictionary<Guid, List<Guid>> BuildConditionalDependencies(QuestionnaireDocument questionnaireDocument, Dictionary<string, Guid> variableNames)
        {
            Dictionary<Guid, List<Guid>> dependencies = questionnaireDocument.GetAllGroups()
                .Where(x => !string.IsNullOrWhiteSpace(x.ConditionExpression))
                .ToDictionary(x => x.PublicKey,
                    x => this.GetIdsOfQuestionsInvolvedInExpression(x.ConditionExpression, variableNames));

            questionnaireDocument.GetEntitiesByType<IQuestion>()
                .Where(x => !string.IsNullOrWhiteSpace(x.ConditionExpression))
                .ToDictionary(x => x.PublicKey,
                    x => this.GetIdsOfQuestionsInvolvedInExpression(x.ConditionExpression, variableNames))
                .ToList()
                .ForEach(x => dependencies.Add(x.Key, x.Value));

            return dependencies;
        }

        private List<Guid> GetIdsOfQuestionsInvolvedInExpression(string conditionExpression, Dictionary<string, Guid> variableNames)
        {
            return new List<Guid>(
                from variable in this.ExpressionProcessor.GetIdentifiersUsedInExpression(conditionExpression)
                where variableNames.ContainsKey(variable)
                select variableNames[variable]);
        }

        private static bool IsRosterGroup(IGroup group)
        {
            return group.IsRoster || group.Propagated == Propagate.AutoPropagated;
        }

        private string GenerateTypeNameByScope(IEnumerable<Guid> currentRosterScope, Dictionary<string, string> generatedScopesTypeNames)
        {
            var scopeStringKey = String.Join("$", currentRosterScope);
            if (!generatedScopesTypeNames.ContainsKey(scopeStringKey))
                generatedScopesTypeNames.Add(scopeStringKey, "@__" + Guid.NewGuid().FormatGuid());

            return generatedScopesTypeNames[scopeStringKey];
        }
    }
}