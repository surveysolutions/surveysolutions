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
        private List<Guid> ConditionExecutionOrder { set; get; }


        public QuestionnaireExecutorTemplateModel(QuestionnaireDocument questionnaireDocument)
        {
            this.AllQuestions = new List<QuestionTemplateModel>();
            this.AllGroups = new List<GroupTemplateModel>();
            this.AllRosters = new List<RosterTemplateModel>();

            this.GeneratedScopesTypeNames = new Dictionary<string, string>();

            this.QuestionnaireLevelModel = new QuestionnaireLevelTemplateModel(this);

            this.Id = questionnaireDocument.PublicKey;

            this.GeneratedClassName = String.Format("{0}_{1}", InterviewExpressionStatePrefix, Guid.NewGuid().FormatGuid());

            this.BuildStructures(questionnaireDocument);
            
            this.StructuralDependencies = questionnaireDocument
                .GetAllGroups()
                .ToDictionary(group => @group.PublicKey, group => @group.Children.Select(x => x.PublicKey).ToList());

            this.VariableNames = AllQuestions.ToDictionary(q => q.VariableName, q => q.Id);
            AllRosters.ForEach(r => this.VariableNames.Add(r.VariableName, Id));

            this.BuildConditionalDependencies(questionnaireDocument);

        }


        private void BuildStructures(QuestionnaireDocument questionnaireDoc)
        {
            var rostersToProcess = new Queue<Tuple<IGroup, IRosterScope>>();
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

        private void BuildConditionalDependencies(QuestionnaireDocument questionnaireDocument)
        {
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

            this.ConditionalDependencies = dependencies;
        }


        private List<Guid> GetIdsOfQuestionsInvolvedInExpression(string conditionExpression)
        {
            //char[] separators = { '.', ')', '(' , ' ', '=' , '!', '[', ']', ',', '<', '>', '?', ':', '+', '-'};
            //string[] expressionStrings = conditionExpression.Split(separators);
            //return VariableNames.Where(v => expressionStrings.Contains(v.Key, StringComparer.Ordinal)).Select(d => d.Value).ToList();
            
            return VariableNames.Where(v => conditionExpression.IndexOf(v.Key, StringComparison.Ordinal) > -1).Select(d => d.Value).ToList();
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

        public List<Tuple<string, string>> GetOrderedListByConditionDependency(List<QuestionTemplateModel> questions, List<GroupTemplateModel> groups)
        {
            List<GroupTemplateModel> groupsWithConditions = groups.Where(g => !string.IsNullOrWhiteSpace(g.Conditions)).Reverse().ToList();
            var questionsWithConditions = questions.Where(q => !string.IsNullOrWhiteSpace(q.Conditions)).ToList();

            Dictionary<Guid, Tuple<string, string>> itemsToSort = 
                groupsWithConditions.ToDictionary(g => g.Id, g => new Tuple<string, string>(g.GeneratedConditionsMethodName, g.GeneratedStateName));
            questionsWithConditions.ForEach(q => itemsToSort.Add(q.Id, new Tuple<string, string>(q.GeneratedConditionsMethodName, q.GeneratedStateName)));

            HashSet<Guid> processedQuestion = new HashSet<Guid>();
            List<Guid> orderedList = new List<Guid>();
            var conditionalStack = new Stack<Guid>();
            
            foreach (var item in questionsWithConditions)
            {
                conditionalStack.Push(item.Id);
            }

            foreach (var item in groupsWithConditions)
            {
                conditionalStack.Push(item.Id);
            }

            while (conditionalStack.Any())
            {
                var currentNode = conditionalStack.Peek();

                if (!orderedList.Contains(currentNode))
                {
                    var dependencies = GetQuestionsInvolvedInConditionsFromCurrentScope(currentNode, questions, processedQuestion).ToList();
                    if (dependencies.Any())
                    {
                        foreach (var dependency in dependencies)
                        {
                            conditionalStack.Push(dependency);
                            processedQuestion.Add(dependency);
                        }
                    }
                    else
                    {
                        orderedList.Add(currentNode);
                        conditionalStack.Pop();
                    }
                    
                }
                else
                {
                    conditionalStack.Pop();
                }
            }

            var itemsSorted = new List<Tuple<string, string>>();
            foreach (var id in orderedList)
            {
                if (itemsToSort.ContainsKey(id))
                    itemsSorted.Add(itemsToSort[id]);
            }

            return itemsSorted;
        }

        private IEnumerable<Guid> GetQuestionsInvolvedInConditionsFromCurrentScope(Guid currentNode, IEnumerable<QuestionTemplateModel> questions, HashSet<Guid> processedQuestion)
        {
            if (!ConditionalDependencies.ContainsKey(currentNode) || (ConditionalDependencies.ContainsKey(currentNode) && ConditionalDependencies[currentNode].Count == 0))
                return new List<Guid>();
                
            var dependencies = ConditionalDependencies[currentNode];

            return dependencies.Intersect(questions.Select(q => q.Id)).Where(g => !processedQuestion.Contains(g));
        }
    }
}
