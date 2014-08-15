using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.GenericSubdomains.Utils;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class QuestionnaireExecutorTemplateModel
    {
        private const string InterviewExpressionStatePrefix = "InterviewExpressionState";

        public Guid Id { private set; get; }

        public List<QuestionTemplateModel> AllQuestions { private set; get; }
        public List<RosterTemplateModel> AllRosters { private set; get; }
        public List<GroupTemplateModel> AllGroups { private set; get; }

        public string GeneratedClassName { private set; get; }

        public Dictionary<string, string> GeneratedScopesTypeNames { private set; get; }

        public Dictionary<string, List<RosterTemplateModel>> RostersGroupedByScope { private set; get; }

        public Dictionary<Guid, Guid[]> ParentsMap { private set; get; }
        public Dictionary<Guid, Guid[]> RostersIdToScopeMap { private set; get; }
        
        public Dictionary<Guid, List<Guid>> ConditionalDependencies { private set; get; }
        public Dictionary<Guid, List<Guid>> StructuralDependencies { private set; get; }

        public QuestionnaireLevelTemplateModel QuestionnaireLevelModel { private set; get; }

        private Dictionary<string, Guid> VariableNames { set; get; }


        public QuestionnaireExecutorTemplateModel(QuestionnaireDocument questionnaireDocument)
        {
            this.AllQuestions = new List<QuestionTemplateModel>();
            this.AllGroups = new List<GroupTemplateModel>();
            this.AllRosters = new List<RosterTemplateModel>();

            this.GeneratedScopesTypeNames = new Dictionary<string, string>();

            this.QuestionnaireLevelModel = new QuestionnaireLevelTemplateModel();

            this.Id = questionnaireDocument.PublicKey;

            this.GeneratedClassName = String.Format("{0}_{1}", InterviewExpressionStatePrefix, Guid.NewGuid().FormatGuid());

            this.BuildStructures(questionnaireDocument);
            this.StructuralDependencies = questionnaireDocument
                .GetAllGroups()
                .ToDictionary(group => @group.PublicKey, group => @group.Children.Select(x => x.PublicKey).ToList());


            this.VariableNames = AllQuestions.ToDictionary(q => q.VariableName, q => q.Id);
            AllRosters.ForEach(r => this.VariableNames.Add(r.VariableName, Id));

            this.ConditionalDependencies = this.BuildDependencyTree(questionnaireDocument);

            }

        private void BuildStructures(QuestionnaireDocument questionnaireDoc)
        {
            Queue<Tuple<IGroup, IRosterScope>> rostersToProcess = new Queue<Tuple<IGroup, IRosterScope>>();
            rostersToProcess.Enqueue(new Tuple<IGroup, IRosterScope>(questionnaireDoc, this.QuestionnaireLevelModel));

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

                        this.AllQuestions.Add(question);

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
                                GeneratedTypeName = this.GenerateTypeNameByScope(currentRosterScope),
                                GeneratedStateName = "@__" + varName + "_state",
                                ParentScope = currentScope,
                                GeneratedIdName = "@__" + varName + "_id",
                                GeneratedConditionsMethodName = "IsEnabled_" + varName,
                                RosterScope = currentRosterScope,
                                GeneratedRosterScopeName = "@__" + varName + "_scope",
                            };

                            rostersToProcess.Enqueue(new Tuple<IGroup, IRosterScope>(childAsIGroup, roster));
                            this.AllRosters.Add(roster);
                            currentScope.Rosters.Add(roster);

                            continue;
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
                            this.AllGroups.Add(group);
                            foreach (var childGroup in childAsIGroup.Children)
                            {
                                childrenOfCurrentRoster.Enqueue(childGroup);
                            }
                        }
                    }
                }
            }

            this.RostersGroupedByScope = this.AllRosters.GroupBy(r => r.GeneratedTypeName).ToDictionary(g => g.Key, g => g.ToList());
        }

        private string GenerateTypeNameByScope(IEnumerable<Guid> currentRosterScope)
        {
            var scopeStringKey = String.Join("$", currentRosterScope);
            if (!this.GeneratedScopesTypeNames.ContainsKey(scopeStringKey))
                this.GeneratedScopesTypeNames.Add(scopeStringKey, "@__" + Guid.NewGuid().FormatGuid());

            return this.GeneratedScopesTypeNames[scopeStringKey];
        }

        private static bool IsRosterGroup(IGroup group)
        {
            return group.IsRoster || group.Propagated == Propagate.AutoPropagated;
        }

        private Dictionary<Guid, List<Guid>> BuildDependencyTree(QuestionnaireDocument questionnaireDocument)
        {
            /*Dictionary<Guid, List<Guid>> dependencies = questionnaireDocument
                .GetAllGroups()
                .ToDictionary(group => @group.PublicKey, group => @group.Children.Select(x => x.PublicKey).ToList());

            questionnaireDocument
                .GetEntitiesByType<IQuestion>()
                .ToDictionary(group => @group.PublicKey, group => new List<Guid>())
                .ToList()
                .ForEach(x => dependencies.Add(x.Key, x.Value));

            Dictionary<Guid, List<Guid>> invertedDependencies = questionnaireDocument.GetAllGroups()
                .Where(x => !string.IsNullOrWhiteSpace(x.ConditionExpression))
                .ToDictionary(x => x.PublicKey,
                    x => this.GetIdsOfQuestionsInvolvedInExpression(x.ConditionExpression));

            questionnaireDocument.GetEntitiesByType<IQuestion>()
                .Where(x => !string.IsNullOrWhiteSpace(x.ConditionExpression))
                .ToDictionary(x => x.PublicKey,
                    x => this.GetIdsOfQuestionsInvolvedInExpression(x.ConditionExpression))
                .ToList()
                .ForEach(x => invertedDependencies.Add(x.Key, x.Value));

            foreach (KeyValuePair<Guid, List<Guid>> dependency in invertedDependencies)
            {
                dependency.Value.ForEach(x => dependencies[x].Add(dependency.Key));
            }*/

            Dictionary<Guid, List<Guid>> dependencies = questionnaireDocument.GetAllGroups()
                .Where(x => !string.IsNullOrWhiteSpace(x.ConditionExpression))
                .ToDictionary(x => x.PublicKey,
                    x => this.GetIdsOfQuestionsInvolvedInExpression(x.ConditionExpression));

            questionnaireDocument.GetEntitiesByType<IQuestion>()
                .Where(x => !string.IsNullOrWhiteSpace(x.ConditionExpression))
                .ToDictionary(x => x.PublicKey,
                    x => this.GetIdsOfQuestionsInvolvedInExpression(x.ConditionExpression))
                .ToList()
                .ForEach(x => dependencies.Add(x.Key, x.Value));

            return dependencies;
        }


        private List<Guid> GetIdsOfQuestionsInvolvedInExpression(string conditionExpression)
        {
            //totally not the best way to do this
            char[] separators = { '.', ')', '(' , ' ', '=' , '!', '[', ']'};

            string[] expressionStrings = conditionExpression.Split(separators);

            return VariableNames.Where(v => expressionStrings.Contains(v.Key, StringComparer.Ordinal)).Select(d => d.Value).ToList();
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

        public static List<Tuple<string, string>> GetOrderedListByConditionDependency(List<QuestionTemplateModel> questions,
            List<GroupTemplateModel> groups,
            Dictionary<Guid, Guid[]> conditionalDependencies)
        {
            var sortedList = (from groupTemplateModel
                in groups
                where !string.IsNullOrWhiteSpace(groupTemplateModel.Conditions)
                select new Tuple<string, string>(groupTemplateModel.GeneratedConditionsMethodName, groupTemplateModel.GeneratedStateName))
                .ToList();

            sortedList.AddRange(from questionTemplateModel
                in questions
                where !string.IsNullOrWhiteSpace(questionTemplateModel.Conditions)
                select
                    new Tuple<string, string>(questionTemplateModel.GeneratedConditionsMethodName, questionTemplateModel.GeneratedStateName));


            return sortedList;
        }
    }
}
