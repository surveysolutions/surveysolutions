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
                null, 
                null, 
                null) {}

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
                staticTextsToBeDisabled, staticTextsToBeEnabled, 
                null, 
                null)
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

        private static EnablementChanges Union(EnablementChanges first, EnablementChanges second)
        {
            first.GroupsToBeDisabled.AddRange(second.GroupsToBeDisabled);
            first.GroupsToBeEnabled.AddRange(second.GroupsToBeEnabled);
            first.QuestionsToBeDisabled.AddRange(second.QuestionsToBeDisabled);
            first.QuestionsToBeEnabled.AddRange(second.QuestionsToBeEnabled);
            first.StaticTextsToBeDisabled.AddRange(second.StaticTextsToBeDisabled);
            first.StaticTextsToBeEnabled.AddRange(second.StaticTextsToBeEnabled);
            first.VariablesToBeDisabled.AddRange(second.VariablesToBeDisabled);
            first.VariablesToBeEnabled.AddRange(second.VariablesToBeEnabled);
            return first;
        }

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