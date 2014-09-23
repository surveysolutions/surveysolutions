using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class QuestionnaireExecutorTemplateModel
    {
        public Guid Id { set; get; }

        public List<QuestionTemplateModel> AllQuestions { set; get; }
        public List<GroupTemplateModel> AllGroups { set; get; }
        public List<RosterTemplateModel> AllRosters { set; get; }

        public string GeneratedClassName { set; get; }
        public Dictionary<string, string> GeneratedScopesTypeNames { set; get; }

        public Dictionary<string, List<RosterTemplateModel>> RostersGroupedByScope { set; get; }

        public Dictionary<Guid, List<Guid>> ConditionalDependencies { set; get; }
        public Dictionary<Guid, List<Guid>> StructuralDependencies { set; get; }

        public QuestionnaireLevelTemplateModel QuestionnaireLevelModel { set; get; }

        public Dictionary<string, Guid> VariableNames { set; get; }

        
        public List<Tuple<string, string>> GetOrderedListByConditionDependency(List<QuestionTemplateModel> questions,
            List<GroupTemplateModel> groups, List<RosterTemplateModel> rosters = null)
        {
            var conditionalDependencies = this.ConditionalDependencies;

            var groupsWithConditions = groups.Where(g => !string.IsNullOrWhiteSpace(g.Conditions)).Reverse().ToList();
            var questionsWithConditions = questions.Where(q => !string.IsNullOrWhiteSpace(q.Conditions)).ToList();
            var rostersWithConditions = rosters != null
                ? rosters.Where(r => !string.IsNullOrWhiteSpace(r.Conditions)).Reverse().ToList()
                : new List<RosterTemplateModel>();

            Dictionary<Guid, Tuple<string, string>> itemsToSort =
                groupsWithConditions.ToDictionary(g => g.Id,
                    g => new Tuple<string, string>(g.GeneratedConditionsMethodName, g.GeneratedStateName));

            rostersWithConditions.ForEach(
                r => itemsToSort.Add(r.Id, new Tuple<string, string>(r.GeneratedConditionsMethodName, r.GeneratedStateName)));

            questionsWithConditions.ForEach(
                q => itemsToSort.Add(q.Id, new Tuple<string, string>(q.GeneratedConditionsMethodName, q.GeneratedStateName)));

            var processedQuestion = new HashSet<Guid>();
            var orderedList = new List<Guid>();
            var conditionalStack = new Stack<Guid>();

            foreach (var item in questionsWithConditions)
            {
                conditionalStack.Push(item.Id);
            }

            foreach (var item in rostersWithConditions)
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
                    var dependencies =
                        GetQuestionsInvolvedInConditionsFromCurrentScope(currentNode, questions, processedQuestion, conditionalDependencies)
                            .ToList();
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

        private static IEnumerable<Guid> GetQuestionsInvolvedInConditionsFromCurrentScope(Guid currentNode,
            IEnumerable<QuestionTemplateModel> questions, HashSet<Guid> processedQuestion,
            Dictionary<Guid, List<Guid>> conditionalDependencies)
        {
            if (!conditionalDependencies.ContainsKey(currentNode) ||
                (conditionalDependencies.ContainsKey(currentNode) && conditionalDependencies[currentNode].Count == 0))
                return new List<Guid>();

            var dependencies = conditionalDependencies[currentNode];

            return dependencies.Intersect(questions.Select(q => q.Id)).Where(g => !processedQuestion.Contains(g));
        }
    }
}
