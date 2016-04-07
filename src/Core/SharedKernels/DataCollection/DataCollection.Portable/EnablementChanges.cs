using System.Collections.Generic;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection
{
    public class EnablementChanges
    {
        internal EnablementChanges() : this(null, null, null, null, null, null) {}

        public EnablementChanges(
            List<Identity> groupsToBeDisabled,
            List<Identity> groupsToBeEnabled,
            List<Identity> questionsToBeDisabled,
            List<Identity> questionsToBeEnabled,
            List<Identity> staticTextsToBeDisabled = null,
            List<Identity> staticTextsToBeEnabled = null)
        {
            this.GroupsToBeDisabled = groupsToBeDisabled ?? new List<Identity>();
            this.GroupsToBeEnabled = groupsToBeEnabled ?? new List<Identity>();
            this.QuestionsToBeDisabled = questionsToBeDisabled ?? new List<Identity>();
            this.QuestionsToBeEnabled = questionsToBeEnabled ?? new List<Identity>();
            this.StaticTextsToBeDisabled = staticTextsToBeDisabled ?? new List<Identity>();
            this.StaticTextsToBeEnabled = staticTextsToBeEnabled ?? new List<Identity>();
        }

        public List<Identity> GroupsToBeDisabled { get; }
        public List<Identity> GroupsToBeEnabled { get; }
        public List<Identity> QuestionsToBeDisabled { get; }
        public List<Identity> QuestionsToBeEnabled { get; }
        public List<Identity> StaticTextsToBeDisabled { get; }
        public List<Identity> StaticTextsToBeEnabled { get; }

        public static EnablementChanges Union(EnablementChanges first, EnablementChanges second)
            => new EnablementChanges(
                first.GroupsToBeDisabled.Union(second.GroupsToBeDisabled, IdentityComparer.Instance).ToList(),
                first.GroupsToBeEnabled.Union(second.GroupsToBeEnabled, IdentityComparer.Instance).ToList(),
                first.QuestionsToBeDisabled.Union(second.QuestionsToBeDisabled, IdentityComparer.Instance).ToList(),
                first.QuestionsToBeEnabled.Union(second.QuestionsToBeEnabled, IdentityComparer.Instance).ToList(),
                first.StaticTextsToBeDisabled.Union(second.StaticTextsToBeDisabled, IdentityComparer.Instance).ToList(),
                first.StaticTextsToBeEnabled.Union(second.StaticTextsToBeEnabled, IdentityComparer.Instance).ToList());

        public static EnablementChanges Union(IEnumerable<EnablementChanges> manyChanges)
            => manyChanges.Aggregate(new EnablementChanges(), Union);

        public void Clear()
        {
            this.GroupsToBeEnabled.Clear();
            this.GroupsToBeDisabled.Clear();
            this.QuestionsToBeEnabled.Clear();
            this.QuestionsToBeDisabled.Clear();
            this.StaticTextsToBeEnabled.Clear();
            this.StaticTextsToBeDisabled.Clear();
        }
    }
}