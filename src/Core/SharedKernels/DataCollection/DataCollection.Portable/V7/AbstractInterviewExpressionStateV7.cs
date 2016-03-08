using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.V2;
using WB.Core.SharedKernels.DataCollection.V4;
using WB.Core.SharedKernels.DataCollection.V5;
using WB.Core.SharedKernels.DataCollection.V6;

namespace WB.Core.SharedKernels.DataCollection.V7
{
    public abstract class AbstractInterviewExpressionStateV7 : AbstractInterviewExpressionStateV6, IInterviewExpressionStateV7
    {
        public AbstractInterviewExpressionStateV7()
        {
        }

        protected Dictionary<Identity, LinkedOptionConditionalState> LinkedQuestionOptionsEnablementStates =
            new Dictionary<Identity, LinkedOptionConditionalState>();
        #region methods using InterviewScopes should be overriden
        public new Dictionary<string, IExpressionExecutableV7> InterviewScopes = new Dictionary<string, IExpressionExecutableV7>();

        public AbstractInterviewExpressionStateV7(Dictionary<string, IExpressionExecutableV7> interviewScopes,
            Dictionary<string, List<string>> siblingRosters, IInterviewProperties interviewProperties)
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

        public new IEnumerable<IExpressionExecutableV7> GetRosterInstances(Identity[] rosterKey, Guid scopeId)
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
            foreach (var linkedQuestionOptionsEnablementState in this.LinkedQuestionOptionsEnablementStates.Values)
            {
                linkedQuestionOptionsEnablementState.PreviousState = linkedQuestionOptionsEnablementState.State;
                linkedQuestionOptionsEnablementState.State = State.Unknown;
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

        #endregion

        public LinkedQuestionOptionsChanges ProcessLinkedQuestionFilters()
        {
            var result = new LinkedQuestionOptionsChanges();
            foreach (var interviewScopeKvpValue in this.InterviewScopes.Values)
            {
                var linkedQuestionFilterResults = interviewScopeKvpValue.ExecuteLinkedQuestionFilters();
                var enabledLinkedQuestionOptions =
                    linkedQuestionFilterResults.Where(o=>o.Enabled).Where(o => IsOptionInOtherState(o, State.Enabled))
                        .Select(
                            o =>
                                new LinkedQuestionOption()
                                {
                                    LinkedQuestionId = o.LinkedQuestionId,
                                    RosterVector = o.RosterKey.First().RosterVector
                                })
                        .ToArray();
                var disabledLinkedQuestionOptions =
                 linkedQuestionFilterResults.Where(o => !o.Enabled).Where(o=> IsOptionInOtherState(o, State.Disabled))
                     .Select(
                         o =>
                             new LinkedQuestionOption()
                             {
                                 LinkedQuestionId = o.LinkedQuestionId,
                                 RosterVector = o.RosterKey.First().RosterVector
                             })
                     .ToArray();
                result.OptionsDeclaredEnabled.AddRange(enabledLinkedQuestionOptions);
                result.OptionsDeclaredDisabled.AddRange(disabledLinkedQuestionOptions);
            }
            return result;
        }

        private bool IsOptionInOtherState(LinkedQuestionFilterResult linkedQuestionFilterResult, State state)
        {
            var identity = new Identity(linkedQuestionFilterResult.LinkedQuestionId, linkedQuestionFilterResult.RosterKey.First().RosterVector);
            if (!this.LinkedQuestionOptionsEnablementStates.ContainsKey(identity))
                return true;
            if (this.LinkedQuestionOptionsEnablementStates[identity].PreviousState == state)
                return false;
            return true;
        }

        public void EnableLinkedQuestionOptions(Identity[] options)
        {
            foreach (var identity in options)
            {
                if (!this.LinkedQuestionOptionsEnablementStates.ContainsKey(identity))
                {
                    this.LinkedQuestionOptionsEnablementStates[identity] = new LinkedOptionConditionalState()
                    {
                        OptionIdentity = identity
                    };
                }
                this.LinkedQuestionOptionsEnablementStates[identity].State = State.Enabled;
            }
        }

        public void DisableLinkedQuestionOptions(Identity[] options)
        {
            foreach (var identity in options)
            {
                if (!this.LinkedQuestionOptionsEnablementStates.ContainsKey(identity))
                {
                    this.LinkedQuestionOptionsEnablementStates[identity] = new LinkedOptionConditionalState() { OptionIdentity = identity };
                }
                this.LinkedQuestionOptionsEnablementStates[identity].State = State.Disabled;
            }
        }

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

        IInterviewExpressionStateV7 IInterviewExpressionStateV7.Clone()
        {
            return Clone() as IInterviewExpressionStateV7;
        }
    }
}