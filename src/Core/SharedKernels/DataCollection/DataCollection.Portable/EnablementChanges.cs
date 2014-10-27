using System.Collections.Generic;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection
{
    public class EnablementChanges
    {
        internal EnablementChanges() : this(null, null, null, null) {}

        public EnablementChanges(List<Identity> groupsToBeDisabled, List<Identity> groupsToBeEnabled,
            List<Identity> questionsToBeDisabled, List<Identity> questionsToBeEnabled)
        {
            this.GroupsToBeDisabled = groupsToBeDisabled ?? new List<Identity>();
            this.GroupsToBeEnabled = groupsToBeEnabled ?? new List<Identity>();
            this.QuestionsToBeDisabled = questionsToBeDisabled ?? new List<Identity>();
            this.QuestionsToBeEnabled = questionsToBeEnabled ?? new List<Identity>();
        }

        public List<Identity> GroupsToBeDisabled { get; private set; }
        public List<Identity> GroupsToBeEnabled { get; private set; }
        public List<Identity> QuestionsToBeDisabled { get; private set; }
        public List<Identity> QuestionsToBeEnabled { get; private set; }

        public static EnablementChanges UnionAllEnablementChanges(IEnumerable<EnablementChanges> enablements)
        {
            var groupsToBeDisabled = new List<Identity>();
            var groupsToBeEnabled = new List<Identity>();
            var questionsToBeDisabled = new List<Identity>();
            var questionsToBeEnabled = new List<Identity>();

            IEqualityComparer<Identity> comparer = new IdentityComparer();

            foreach (var enablementChange in enablements)
            {
                groupsToBeDisabled = groupsToBeDisabled.Union(enablementChange.GroupsToBeDisabled, comparer).ToList();
                groupsToBeEnabled = groupsToBeEnabled.Union(enablementChange.GroupsToBeEnabled, comparer).ToList();
                questionsToBeDisabled = questionsToBeDisabled.Union(enablementChange.QuestionsToBeDisabled, comparer).ToList();
                questionsToBeEnabled = questionsToBeEnabled.Union(enablementChange.QuestionsToBeEnabled, comparer).ToList();
            }

            var resultChanges = new EnablementChanges(groupsToBeDisabled, groupsToBeEnabled, questionsToBeDisabled, questionsToBeEnabled);

            return resultChanges;
        }

        public void Clear()
        {
            this.GroupsToBeEnabled.Clear();
            this.GroupsToBeDisabled.Clear();
            this.QuestionsToBeEnabled.Clear();
            this.QuestionsToBeDisabled.Clear();
        }
    }
}