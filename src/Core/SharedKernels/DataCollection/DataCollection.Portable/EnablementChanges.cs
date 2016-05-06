using System.Collections.Generic;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection
{
    public class EnablementChanges
    {
        internal EnablementChanges() : this(null, null, null, null, null, null, null, null) {}

        public EnablementChanges(
            List<Identity> groupsToBeDisabled,
            List<Identity> groupsToBeEnabled,
            List<Identity> questionsToBeDisabled,
            List<Identity> questionsToBeEnabled)
            : this(
                groupsToBeDisabled,
                groupsToBeEnabled,
                questionsToBeDisabled,
                questionsToBeEnabled,
                null,
                null, null, null) {}
        public EnablementChanges(
            List<Identity> groupsToBeDisabled,
            List<Identity> groupsToBeEnabled,
            List<Identity> questionsToBeDisabled,
            List<Identity> questionsToBeEnabled,
            List<Identity> staticTextsToBeDisabled,
            List<Identity> staticTextsToBeEnabled): this(
                groupsToBeDisabled,
                groupsToBeEnabled,
                questionsToBeDisabled,
                questionsToBeEnabled,
                staticTextsToBeDisabled, staticTextsToBeEnabled, null, null)
        {
        }

        public EnablementChanges(
            List<Identity> groupsToBeDisabled,
            List<Identity> groupsToBeEnabled,
            List<Identity> questionsToBeDisabled,
            List<Identity> questionsToBeEnabled,
            List<Identity> staticTextsToBeDisabled,
            List<Identity> staticTextsToBeEnabled,
            List<Identity> variablesToBeDisabled,
            List<Identity> variablesToBeEnabled)
        {
            this.GroupsToBeDisabled = groupsToBeDisabled ?? new List<Identity>();
            this.GroupsToBeEnabled = groupsToBeEnabled ?? new List<Identity>();
            this.QuestionsToBeDisabled = questionsToBeDisabled ?? new List<Identity>();
            this.QuestionsToBeEnabled = questionsToBeEnabled ?? new List<Identity>();
            this.StaticTextsToBeDisabled = staticTextsToBeDisabled ?? new List<Identity>();
            this.StaticTextsToBeEnabled = staticTextsToBeEnabled ?? new List<Identity>();
            this.VariablesToBeDisabled= variablesToBeDisabled??new List<Identity>();
            this.VariablesToBeEnabled= variablesToBeEnabled??new List<Identity>();
        }

        public List<Identity> GroupsToBeDisabled { get; }
        public List<Identity> GroupsToBeEnabled { get; }
        public List<Identity> QuestionsToBeDisabled { get; }
        public List<Identity> QuestionsToBeEnabled { get; }
        public List<Identity> StaticTextsToBeDisabled { get; }
        public List<Identity> StaticTextsToBeEnabled { get; }
        public List<Identity> VariablesToBeDisabled { get; }
        public List<Identity> VariablesToBeEnabled { get; }

        public static EnablementChanges Union(EnablementChanges first, EnablementChanges second)
            => new EnablementChanges(
                first.GroupsToBeDisabled.Union(second.GroupsToBeDisabled, IdentityComparer.Instance).ToList(),
                first.GroupsToBeEnabled.Union(second.GroupsToBeEnabled, IdentityComparer.Instance).ToList(),
                first.QuestionsToBeDisabled.Union(second.QuestionsToBeDisabled, IdentityComparer.Instance).ToList(),
                first.QuestionsToBeEnabled.Union(second.QuestionsToBeEnabled, IdentityComparer.Instance).ToList(),
                first.StaticTextsToBeDisabled.Union(second.StaticTextsToBeDisabled, IdentityComparer.Instance).ToList(),
                first.StaticTextsToBeEnabled.Union(second.StaticTextsToBeEnabled, IdentityComparer.Instance).ToList(),
                first.VariablesToBeDisabled.Union(second.VariablesToBeDisabled, IdentityComparer.Instance).ToList(),
                first.VariablesToBeEnabled.Union(second.VariablesToBeEnabled, IdentityComparer.Instance).ToList());

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
            this.VariablesToBeDisabled.Clear();
            this.VariablesToBeEnabled.Clear();
        }
    }
}