using System;
using System.Collections.Generic;
using System.Linq;
using CsQuery.ExtensionMethods;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
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
        public QuestionnaireLevelTemplateModel QuestionnaireLevelModel { set; get; }
        public Dictionary<string, Guid> VariableNames { set; get; }

        public Dictionary<Guid, List<Guid>> ConditionalDependencies { set; get; }
        public Dictionary<Guid, List<Guid>> StructuralDependencies { set; get; }

        public List<Tuple<string, string>> GetOrderedListByConditionDependency(List<QuestionTemplateModel> questions,
            List<GroupTemplateModel> groups, List<RosterTemplateModel> rosters = null)
        {
            return OrderedListByConditionDependency(this.ConditionalDependencies, questions, groups, rosters);
        }

        private static List<Tuple<string, string>> OrderedListByConditionDependency(Dictionary<Guid, List<Guid>> conditionalDependencies, List<QuestionTemplateModel> questions, List<GroupTemplateModel> groups, List<RosterTemplateModel> rosters)
        {
            List<GroupTemplateModel> groupsWithConditions = groups.Where(g => !string.IsNullOrWhiteSpace(g.Conditions)).Reverse().ToList();
            List<QuestionTemplateModel> questionsWithConditions = questions.Where(q => !string.IsNullOrWhiteSpace(q.Conditions)).ToList();
            
            List<RosterTemplateModel> rostersWithConditions = rosters != null
                ? rosters.Where(r => !string.IsNullOrWhiteSpace(r.Conditions)).Reverse().ToList()
                : new List<RosterTemplateModel>();

            Dictionary<Guid, Tuple<string, string>> itemsToSort =
                groupsWithConditions.ToDictionary(g => g.Id, g => new Tuple<string, string>(g.GeneratedConditionsMethodName, g.GeneratedStateName));

            rostersWithConditions.ForEach(r => itemsToSort.Add(r.Id, new Tuple<string, string>(r.GeneratedConditionsMethodName, r.GeneratedStateName)));

            questionsWithConditions.ForEach(q => itemsToSort.Add(q.Id, new Tuple<string, string>(q.GeneratedConditionsMethodName, q.GeneratedStateName)));

            var orderedList = GetOrderedByConditionDependencyList(
                conditionalDependencies, questions, questionsWithConditions, rostersWithConditions, groupsWithConditions);

            var itemsSorted = new List<Tuple<string, string>>();
            foreach (Guid id in orderedList)
            {
                if (itemsToSort.ContainsKey(id))
                    itemsSorted.Add(itemsToSort[id]);
            }

            return itemsSorted;
        }

        private static IEnumerable<Guid> GetOrderedByConditionDependencyList(Dictionary<Guid, List<Guid>> conditionalDependencies, List<QuestionTemplateModel> questions, List<QuestionTemplateModel> questionsWithConditions,
            IEnumerable<RosterTemplateModel> rostersWithConditions, IEnumerable<GroupTemplateModel> groupsWithConditions)
        {
            var processedQuestion = new HashSet<Guid>();
            var orderedList = new List<Guid>();
            var conditionalStack = new Stack<Guid>();

            questionsWithConditions.ForEach(q => conditionalStack.Push(q.Id));
            rostersWithConditions.ForEach(r => conditionalStack.Push(r.Id));
            groupsWithConditions.ForEach(g => conditionalStack.Push(g.Id));
            
            while (conditionalStack.Any())
            {
                Guid currentNode = conditionalStack.Peek();
                if (!orderedList.Contains(currentNode))
                {
                    List<Guid> dependencies = GetQuestionsInvolvedInConditionsFromCurrentScope(currentNode, questions, processedQuestion, conditionalDependencies).ToList();
                    if (dependencies.Any())
                    {
                        foreach (Guid dependency in dependencies)
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
            return orderedList;
        }

        private static IEnumerable<Guid> GetQuestionsInvolvedInConditionsFromCurrentScope(Guid currentNode, IEnumerable<QuestionTemplateModel> questions, 
            HashSet<Guid> processedQuestion, Dictionary<Guid, List<Guid>> conditionalDependencies)
        {
            if (!conditionalDependencies.ContainsKey(currentNode) ||
                (conditionalDependencies.ContainsKey(currentNode) && conditionalDependencies[currentNode].Count == 0))
                return new List<Guid>();

            List<Guid> dependencies = conditionalDependencies[currentNode];

            return dependencies.Intersect(questions.Select(q => q.Id)).Where(g => !processedQuestion.Contains(g));
        }
    }
}