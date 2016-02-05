using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.V2;
using WB.Core.SharedKernels.DataCollection.V4;
using WB.Core.SharedKernels.DataCollection.V5;

namespace WB.Core.SharedKernels.DataCollection.V6
{
    public abstract class AbstractInterviewExpressionStateV6 : AbstractInterviewExpressionStateV5, IInterviewExpressionStateV6
    {
        public AbstractInterviewExpressionStateV6()
        {
        }

        #region methods using InterviewScopes should be overriden
        public new Dictionary<string, IExpressionExecutableV6> InterviewScopes = new Dictionary<string, IExpressionExecutableV6>();

        public AbstractInterviewExpressionStateV6(Dictionary<string, IExpressionExecutableV6> interviewScopes, Dictionary<string, List<string>> siblingRosters, IInterviewProperties interviewProperties)
        {
            this.InterviewProperties = interviewProperties.Clone();

            var newScopes = interviewScopes.ToDictionary(interviewScope => interviewScope.Key, interviewScope => interviewScope.Value.CopyMembers(this.GetRosterInstances));

            var newSiblingRosters = siblingRosters
                .ToDictionary(
                    interviewScope => interviewScope.Key,
                    interviewScope => new List<string>(interviewScope.Value));


            foreach (var interviewScope in interviewScopes)
            {
                var parent = interviewScope.Value.GetParent();
                if (parent != null)
                    newScopes[interviewScope.Key].SetParent(newScopes[Util.GetRosterStringKey(parent.GetRosterKey())]);

                interviewScope.Value.SetInterviewProperties(this.InterviewProperties);
            }

            this.InterviewScopes = newScopes;
            this.SiblingRosters = newSiblingRosters;
        }


        public new IEnumerable<IExpressionExecutableV6> GetRosterInstances(Identity[] rosterKey, Guid scopeId)
        {
            var siblingsKey = Util.GetSiblingsKey(rosterKey, scopeId);

            var siblingRosters = this.SiblingRosters.ContainsKey(siblingsKey)
                ? this.SiblingRosters[siblingsKey].Select(x => this.InterviewScopes[x])
                : null;

            return siblingRosters;
        }

        protected override IExpressionExecutable GetRosterByIdAndVector(Guid questionId, decimal[] rosterVector)
        {
            var parentsMap = this.GetParentsMap();
            if (!parentsMap.ContainsKey(questionId))
                return null;

            var rosterKey = Util.GetRosterKey(parentsMap[questionId], rosterVector);
            var rosterStringKey = Util.GetRosterStringKey(rosterKey);
            return this.InterviewScopes.ContainsKey(rosterStringKey) ? this.InterviewScopes[rosterStringKey] : null;
        }

        public new void SaveAllCurrentStatesAsPrevious()
        {
            foreach (var interviewScopeKvpValue in this.InterviewScopes.Values.OrderBy(x => x.GetLevel()))
            {
                interviewScopeKvpValue.SaveAllCurrentStatesAsPrevious();
            }
        }

        public new ValidityChanges ProcessValidationExpressions()
        {
            ValidityChanges changes = new ValidityChanges();

            foreach (var interviewScopeKvpValue in this.InterviewScopes.Values)
            {
                changes.AppendChanges(interviewScopeKvpValue.ProcessValidationExpressions());
            }

            return changes;
        }

        public new EnablementChanges ProcessEnablementConditions()
        {
            var questionsToBeEnabled = new List<Identity>();
            var questionsToBeDisabled = new List<Identity>();
            var groupsToBeEnabled = new List<Identity>();
            var groupsToBeDisabled = new List<Identity>();

            //order by scope depth starting from top
            //conditionally lower scope could depend only from upper scope
            foreach (var interviewScopeKvpValue in this.InterviewScopes.Values.OrderBy(x => x.GetLevel()))
            {
                List<Identity> questionsToBeEnabledArray;
                List<Identity> questionsToBeDisabledArray;
                List<Identity> groupsToBeEnabledArray;
                List<Identity> groupsToBeDisabledArray;

                interviewScopeKvpValue.CalculateConditionChanges(out questionsToBeEnabledArray, out questionsToBeDisabledArray, out groupsToBeEnabledArray,
                    out groupsToBeDisabledArray);

                questionsToBeEnabled.AddRange(questionsToBeEnabledArray);
                questionsToBeDisabled.AddRange(questionsToBeDisabledArray);
                groupsToBeEnabled.AddRange(groupsToBeEnabledArray);
                groupsToBeDisabled.AddRange(groupsToBeDisabledArray);
            }

            return new EnablementChanges(groupsToBeDisabled, groupsToBeEnabled, questionsToBeDisabled, questionsToBeEnabled);
        }

        public override void AddRoster(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId, int? sortIndex)
        {
            if (!HasParentScropeRosterId(rosterId))
            {
                return;
            }

            decimal[] rosterVector = Util.GetRosterVector(outerRosterVector, rosterInstanceId);
            Guid[] rosterScopeIds = GetParentRosterScopeIds(rosterId);
            var rosterIdentityKey = Util.GetRosterKey(rosterScopeIds, rosterVector);
            string rosterStringKey = Util.GetRosterStringKey(rosterIdentityKey);

            if (this.InterviewScopes.ContainsKey(rosterStringKey))
            {
                return;
            }

            var rosterParentIdentityKey = outerRosterVector.Length == 0
                ? Util.GetRosterKey(new[] { GetQuestionnaireId() }, new decimal[0])
                : Util.GetRosterKey(rosterScopeIds.Shrink(), outerRosterVector);

            var parent = this.InterviewScopes[Util.GetRosterStringKey(rosterParentIdentityKey)];

            var rosterLevel = parent.CreateChildRosterInstance(rosterId, rosterVector, rosterIdentityKey);
            rosterLevel.SetInterviewProperties(this.InterviewProperties);

            this.InterviewScopes.Add(rosterStringKey, rosterLevel);
            this.SetSiblings(rosterIdentityKey, rosterStringKey);
        }

        public new void UpdateRosterTitle(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId,
           string rosterTitle)
        {
            if (!HasParentScropeRosterId(rosterId))
            {
                return;
            }

            decimal[] rosterVector = Util.GetRosterVector(outerRosterVector, rosterInstanceId);
            var rosterIdentityKey = Util.GetRosterKey(GetParentRosterScopeIds(rosterId), rosterVector);
            var rosterStringKey = Util.GetRosterStringKey(rosterIdentityKey);

            var rosterLevel = this.InterviewScopes[rosterStringKey] as IRosterLevel;
            if (rosterLevel != null)
                rosterLevel.SetRowName(rosterTitle);
        }


        public override void RemoveRoster(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId)
        {
            if (!HasParentScropeRosterId(rosterId))
            {
                return;
            }

            decimal[] rosterVector = Util.GetRosterVector(outerRosterVector, rosterInstanceId);
            var rosterIdentityKey = Util.GetRosterKey(GetParentRosterScopeIds(rosterId), rosterVector);

            var dependentRosters = this.InterviewScopes.Keys.Where(x => x.StartsWith(Util.GetRosterStringKey((rosterIdentityKey)))).ToArray();

            foreach (var rosterKey in dependentRosters)
            {
                this.InterviewScopes.Remove(rosterKey);
                foreach (var siblings in this.SiblingRosters.Values)
                {
                    siblings.Remove(rosterKey);
                }
            }
        }

        public void DeclareAnswersValid(IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedValidationConditions)
        {
            this.DeclareAnswersValid(failedValidationConditions.Keys);
        }

        #endregion


        IInterviewExpressionStateV2 IInterviewExpressionStateV2.Clone()
        {
            return Clone() as IInterviewExpressionStateV2;
        }
        IInterviewExpressionStateV4 IInterviewExpressionStateV4.Clone()
        {
            return Clone() as IInterviewExpressionStateV4;
        }

        IInterviewExpressionStateV5 IInterviewExpressionStateV5.Clone()
        {
            return Clone() as IInterviewExpressionStateV5;
        }

        IInterviewExpressionStateV6 IInterviewExpressionStateV6.Clone()
        {
            return Clone() as IInterviewExpressionStateV6;
        }

    }
}