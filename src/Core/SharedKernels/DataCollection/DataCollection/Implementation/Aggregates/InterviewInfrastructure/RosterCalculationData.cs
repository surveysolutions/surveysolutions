using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public class RosterCalculationData
    {
        public RosterCalculationData(List<RosterIdentity> rosterInstancesToAdd,
            Dictionary<decimal, string> titlesForRosterInstancesByInstanceId)
            : this(
                rosterInstancesToAdd: rosterInstancesToAdd,
                titlesForRosterInstancesByInstanceId: titlesForRosterInstancesByInstanceId,
                rosterInstancesToRemove: new List<RosterIdentity>(),
                rosterInstancesToChange: new List<RosterIdentity>(),
                answersToRemoveByDecreasedRosterSize: new List<Identity>(),
                disabledAnswersToEnableByDecreasedRosterSize: new List<Identity>(),
                disabledGroupsToEnableByDecreasedRosterSize: new List<Identity>(),
                rosterInstantiatesFromNestedLevels: new List<RosterCalculationData>()) { }

        public RosterCalculationData(List<RosterIdentity> rosterInstancesToAdd, List<RosterIdentity> rosterInstancesToRemove,
            List<RosterIdentity> rosterInstancesToChange,
            List<Identity> answersToRemoveByDecreasedRosterSize,
            List<Identity> disabledAnswersToEnableByDecreasedRosterSize,
            List<Identity> disabledGroupsToEnableByDecreasedRosterSize,
            Dictionary<decimal, string> titlesForRosterInstancesByInstanceId,
            List<RosterCalculationData> rosterInstantiatesFromNestedLevels)
        {
            this.RosterInstancesToAdd = rosterInstancesToAdd;
            this.RosterInstancesToRemove = rosterInstancesToRemove;
            this.RosterInstancesToChange = rosterInstancesToChange;
            this.AnswersToRemoveByDecreasedRosterSize = answersToRemoveByDecreasedRosterSize;
            this.DisabledAnswersToEnableByDecreasedRosterSize = disabledAnswersToEnableByDecreasedRosterSize;
            this.DisabledGroupsToEnableByDecreasedRosterSize = disabledGroupsToEnableByDecreasedRosterSize;
            this.TitlesForRosterInstancesByInstanceId = titlesForRosterInstancesByInstanceId;
            this.RosterInstantiatesFromNestedLevels = rosterInstantiatesFromNestedLevels;
        }

        private Dictionary<decimal, string> TitlesForRosterInstancesByInstanceId { get; set; }
        private Dictionary<Guid, Dictionary<decimal, string>> TitlesForRosterInstancesByRosterIdAndInstanceId { get; set; }
        public List<RosterIdentity> RosterInstancesToAdd { get; private set; }
        public List<RosterIdentity> RosterInstancesToRemove { get; private set; }
        public List<RosterIdentity> RosterInstancesToChange { get; private set; }
        public List<Identity> AnswersToRemoveByDecreasedRosterSize { get; private set; }
        public List<Identity> DisabledAnswersToEnableByDecreasedRosterSize { get; private set; }
        public List<Identity> DisabledGroupsToEnableByDecreasedRosterSize { get; private set; }
        public List<RosterCalculationData> RosterInstantiatesFromNestedLevels { get; private set; }

        public string GetRosterInstanceTitle(Guid rosterId, decimal rosterInstanceId)
        {
            return this.GetRosterInstanceTitleByRosterIdAndInstanceId(rosterId, rosterInstanceId) ?? this.GetRosterInstanceTitleByInstanceId(rosterInstanceId);
        }

        private string GetRosterInstanceTitleByRosterIdAndInstanceId(Guid rosterId, decimal rosterInstanceId)
        {
            return this.TitlesForRosterInstancesByRosterIdAndInstanceId != null
                && this.TitlesForRosterInstancesByRosterIdAndInstanceId.ContainsKey(rosterId)
                && this.TitlesForRosterInstancesByRosterIdAndInstanceId[rosterId].ContainsKey(rosterInstanceId)
                ? this.TitlesForRosterInstancesByRosterIdAndInstanceId[rosterId][rosterInstanceId]
                : null;
        }

        private string GetRosterInstanceTitleByInstanceId(decimal rosterInstanceId)
        {
            return this.TitlesForRosterInstancesByInstanceId != null
                && this.TitlesForRosterInstancesByInstanceId.ContainsKey(rosterInstanceId)
                ? this.TitlesForRosterInstancesByInstanceId[rosterInstanceId]
                : null;
        }

        public void SetTitlesForRosterInstances(Dictionary<decimal, string> titlesForRosterInstances)
        {
            this.TitlesForRosterInstancesByInstanceId = titlesForRosterInstances;
        }

        public void SetTitlesForRosterInstances(Dictionary<Guid, Dictionary<decimal, string>> titlesForRosterInstances)
        {
            this.TitlesForRosterInstancesByRosterIdAndInstanceId = titlesForRosterInstances;
        }

        public bool AreTitlesForRosterInstancesSpecified()
        {
            return this.TitlesForRosterInstancesByInstanceId != null || this.TitlesForRosterInstancesByRosterIdAndInstanceId != null;
        }
    }
}
