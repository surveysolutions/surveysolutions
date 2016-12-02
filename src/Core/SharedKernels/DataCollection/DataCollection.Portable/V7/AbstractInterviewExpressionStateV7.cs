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
        
        #region methods using InterviewScopes should be overriden
        public new Dictionary<string, IExpressionExecutableV7> InterviewScopes = new Dictionary<string, IExpressionExecutableV7>();

        protected Dictionary<Guid, Guid> linkedQuestionIdWithSourceRosterIdPairs=new Dictionary<Guid, Guid>();

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
        }

        public new ValidityChanges ProcessValidationExpressions() => ProcessValidationExpressionsImpl(this.InterviewScopes.Values);

        public new EnablementChanges ProcessEnablementConditions() => ProcessEnablementConditionsImpl(this.InterviewScopes.Values);

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
            var rosterStringKey = Util.GetRosterStringKey(rosterIdentityKey);

            var dependentRosters = this.InterviewScopes.Keys.Where(x => x.StartsWith(rosterStringKey)).ToArray();

            foreach (var rosterKey in dependentRosters)
            {
                this.InterviewScopes.Remove(rosterKey);
                foreach (var siblings in this.SiblingRosters.Values)
                {
                    siblings.Remove(rosterKey);
                }
            }
        }

        public override void SetInterviewProperties(IInterviewProperties properties)
        {
            this.InterviewProperties = properties;
            var scopes = this.InterviewScopes.Values;

            foreach (var scope in scopes)
            {
                scope.SetInterviewProperties(properties);
            }
        }

        #endregion

        public virtual LinkedQuestionOptionsChanges ProcessLinkedQuestionFilters()
        {
            var result = new LinkedQuestionOptionsChanges();

            var filterResults=new List<LinkedQuestionFilterResult>();

            foreach (var interviewScopeKvpValue in this.InterviewScopes.Values)
            {
                filterResults.AddRange(interviewScopeKvpValue.ExecuteLinkedQuestionFilters());
            }
            var linkedQuestionOptionsGroupedByQuestionId = filterResults.GroupBy(x => x.LinkedQuestionId);
            foreach (var linkedQuestionOptions in linkedQuestionOptionsGroupedByQuestionId)
            {
                var linkedQuestionId = linkedQuestionOptions.Key;

                var newOptionSet =
                    linkedQuestionOptions.Where(o => o.Enabled).Select(o => o.RosterKey.Last().RosterVector).ToArray();
                
                result.LinkedQuestionOptions.Add(linkedQuestionId, newOptionSet);
            }
            var linkedQuestionsToDoubleCheck =
                linkedQuestionIdWithSourceRosterIdPairs.Where(
                    pair => result.LinkedQuestionOptions.All(l => l.Key != pair.Key)).ToArray();
            foreach (var linkedQuestionIdWithSourceRosterIdPair in linkedQuestionsToDoubleCheck)
            {
                if(!this.InterviewScopes.Values.Any(scope=>scope.GetRosterKey().Any(r=>r.Id== linkedQuestionIdWithSourceRosterIdPair.Value)))
                    result.LinkedQuestionOptions.Add(linkedQuestionIdWithSourceRosterIdPair.Key, new RosterVector[0]);
            }
            return result;
        }

        public bool AreLinkedQuestionsSupported()
        {
            return true;
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