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
        public Guid Id { private set; get; }

        public List<QuestionTemplateModel> AllQuestions { private set; get; }
        public List<RosterTemplateModel> AllRosters { private set; get; }
        public List<GroupTemplateModel> AllGroups { private set; get; }

        public Dictionary<string, string> GeneratedScopesTypeNames { private set; get; }

        public Dictionary<string, List<RosterTemplateModel>> RostersGroupedByScope { private set; get; }

        public Dictionary<Guid, Guid[]> ParentsMap { private set; get; }
        public Dictionary<Guid, Guid[]> RostersIdToScopeMap { private set; get; }
        public Dictionary<Guid, List<Guid>> ConditionalDependencies { private set; get; }
        public QuestionnaireLevelTemplateModel QuestionnaireLevelModel { private set; get; }

        
        public QuestionnaireExecutorTemplateModel(QuestionnaireDocument questionnaireDocument)
        {
            AllQuestions = new List<QuestionTemplateModel>();
            AllGroups = new List<GroupTemplateModel>();
            AllRosters = new List<RosterTemplateModel>();

            GeneratedScopesTypeNames = new Dictionary<string, string>();

            this.QuestionnaireLevelModel = new QuestionnaireLevelTemplateModel();
        
            this.Id = questionnaireDocument.PublicKey;
            this.ConditionalDependencies = this.BuildDependencyTree(questionnaireDocument);


            this.BuildStructures(questionnaireDocument);
            
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
                        var varName = !String.IsNullOrEmpty(childAsIQuestion.StataExportCaption) ?
                            childAsIQuestion.StataExportCaption :
                            childAsIQuestion.PublicKey.FormatGuid();
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
                            GeneratedConditionsMethodName = "IsEnabled_"+ varName,
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
                            var varName = childAsIGroup.PublicKey.FormatGuid();

                            var roster = new RosterTemplateModel()
                            {
                                Id = childAsIGroup.PublicKey,
                                Conditions = childAsIGroup.ConditionExpression,
                                VariableName = "@__" + varName, //waiting for merge roster name from default
                                
                                GeneratedTypeName = GenerateTypeNameByScope(currentRosterScope),
                                
                                GeneratedStateName = "@__" + varName + "_state",
                                ParentScope = currentScope,
                                GeneratedIdName = "@__" + varName + "_id",
                                GeneratedConditionsMethodName = "IsEnabled_" + varName,
                                RosterScope = currentRosterScope,
                                GeneratedRosterScopeName = "@__"+ varName + "_scope",

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

            RostersGroupedByScope = AllRosters.GroupBy(r => r.GeneratedTypeName).ToDictionary(g => g.Key, g => g.ToList());
        }

        private string GenerateTypeNameByScope(IEnumerable<Guid> currentRosterScope)
        {
            var scopeStringKey = String.Join("$", currentRosterScope);
            if (!GeneratedScopesTypeNames.ContainsKey(scopeStringKey))
                GeneratedScopesTypeNames.Add(scopeStringKey, "@__" + Guid.NewGuid().FormatGuid());
            
            return GeneratedScopesTypeNames[scopeStringKey];
        }

        private static bool IsRosterGroup(IGroup group)
        {
            return group.Propagated == Propagate.AutoPropagated || group.IsRoster;
        }

        private Dictionary<Guid, List<Guid>> BuildDependencyTree(QuestionnaireDocument questionnaireDocument)
        {
            Dictionary<Guid, List<Guid>> dependencies = questionnaireDocument
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
                    x => this.GetIdsOfQuestionsInvolvedInExpression(x.ConditionExpression, questionnaireDocument));

            questionnaireDocument.GetEntitiesByType<IQuestion>()
                .Where(x => !string.IsNullOrWhiteSpace(x.ConditionExpression))
                .ToDictionary(x => x.PublicKey,
                    x => this.GetIdsOfQuestionsInvolvedInExpression(x.ConditionExpression, questionnaireDocument))
                .ToList()
                .ForEach(x => invertedDependencies.Add(x.Key, x.Value));

            foreach (KeyValuePair<Guid, List<Guid>> dependency in invertedDependencies)
            {
                dependency.Value.ForEach(x => dependencies[x].Add(dependency.Key));
            }

            return dependencies;
        }

        private List<Guid> GetIdsOfQuestionsInvolvedInExpression(string conditionExpression, QuestionnaireDocument questionnaireDocument)
        {
            return new List<Guid>();
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
    }
}
