using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class QuestionnaireExecutorTemplateModel
    {
        public bool GenerateEmbeddedExpressionMethods { get; set; }

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
        public List<Guid> ConditionsPlayOrder { get; set; }

        public string VersionPrefix { get; set; }
        public string[] Namespaces { get; set; }
        public bool ShouldGenerateUpdateRosterTitleMethods { get; set; }

        public List<Tuple<string, string>> GetOrderedListByConditionDependency(List<QuestionTemplateModel> questions,
            List<GroupTemplateModel> groups, List<RosterTemplateModel> rosters = null)
        {
            return OrderedListByConditionDependency(questions, groups, rosters, this.ConditionsPlayOrder);
        }

        private static List<Tuple<string, string>> OrderedListByConditionDependency(List<QuestionTemplateModel> questions, List<GroupTemplateModel> groups, List<RosterTemplateModel> rosters, List<Guid> conditionsPlayOrder )
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

            var itemsSorted = new List<Tuple<string, string>>();

            foreach (Guid id in conditionsPlayOrder)
            {
                if (itemsToSort.ContainsKey(id))
                    itemsSorted.Add(itemsToSort[id]);
            }

            return itemsSorted;
        }
    }
}