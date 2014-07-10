using System.Collections.Generic;
using WB.Core.SharedKernels.ExpressionProcessing;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    internal class RosterCalculationData
    {
        public RosterCalculationData(List<RosterIdentity> rosterInstancesToAdd,
            Dictionary<decimal, string> titlesForRosterInstancesToAdd)
            : this(
                rosterInstancesToAdd: rosterInstancesToAdd,
                titlesForRosterInstancesToAdd: titlesForRosterInstancesToAdd,
                rosterInstancesToRemove: new List<RosterIdentity>(),
                rosterInstancesToChange: new List<RosterIdentity>(),
                answersToRemoveByDecreasedRosterSize: new List<Identity>(),
                initializedGroupsToBeDisabled: new List<Identity>(),
                initializedGroupsToBeEnabled: new List<Identity>(),
                initializedQuestionsToBeDisabled: new List<Identity>(),
                initializedQuestionsToBeEnabled: new List<Identity>(),
                initializedQuestionsToBeInvalid: new List<Identity>(),
                rosterInstantiatesFromNestedLevels: new List<RosterCalculationData>()) { }

        public RosterCalculationData(List<RosterIdentity> rosterInstancesToAdd, List<RosterIdentity> rosterInstancesToRemove,
            List<RosterIdentity> rosterInstancesToChange,
            List<Identity> answersToRemoveByDecreasedRosterSize,
            List<Identity> initializedGroupsToBeDisabled, List<Identity> initializedGroupsToBeEnabled,
            List<Identity> initializedQuestionsToBeDisabled, List<Identity> initializedQuestionsToBeEnabled,
            List<Identity> initializedQuestionsToBeInvalid,
            Dictionary<decimal, string> titlesForRosterInstancesToAdd,
            List<RosterCalculationData> rosterInstantiatesFromNestedLevels)
        {
            this.RosterInstancesToAdd = rosterInstancesToAdd;
            this.RosterInstancesToRemove = rosterInstancesToRemove;
            this.RosterInstancesToChange = rosterInstancesToChange;
            this.AnswersToRemoveByDecreasedRosterSize = answersToRemoveByDecreasedRosterSize;
            this.InitializedGroupsToBeDisabled = initializedGroupsToBeDisabled;
            this.InitializedGroupsToBeEnabled = initializedGroupsToBeEnabled;
            this.InitializedQuestionsToBeDisabled = initializedQuestionsToBeDisabled;
            this.InitializedQuestionsToBeEnabled = initializedQuestionsToBeEnabled;
            this.InitializedQuestionsToBeInvalid = initializedQuestionsToBeInvalid;
            this.TitlesForRosterInstancesToAdd = titlesForRosterInstancesToAdd;
            this.RosterInstantiatesFromNestedLevels = rosterInstantiatesFromNestedLevels;
        }

        public Dictionary<decimal, string> TitlesForRosterInstancesToAdd { get; set; }
        public List<RosterIdentity> RosterInstancesToAdd { get; private set; }
        public List<RosterIdentity> RosterInstancesToRemove { get; private set; }
        public List<RosterIdentity> RosterInstancesToChange { get; private set; }
        public List<Identity> AnswersToRemoveByDecreasedRosterSize { get; private set; }
        public List<Identity> InitializedGroupsToBeDisabled { get; private set; }
        public List<Identity> InitializedGroupsToBeEnabled { get; private set; }
        public List<Identity> InitializedQuestionsToBeDisabled { get; private set; }
        public List<Identity> InitializedQuestionsToBeEnabled { get; private set; }
        public List<Identity> InitializedQuestionsToBeInvalid { get; private set; }
        public List<RosterCalculationData> RosterInstantiatesFromNestedLevels { get; private set; }
    }
}
