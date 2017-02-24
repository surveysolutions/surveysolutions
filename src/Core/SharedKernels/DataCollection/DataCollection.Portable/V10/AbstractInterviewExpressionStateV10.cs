using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.V9;

namespace WB.Core.SharedKernels.DataCollection.V10
{
    public abstract class AbstractInterviewExpressionStateV10 : AbstractInterviewExpressionStateV9, IInterviewExpressionStateV10
    {
        protected AbstractInterviewExpressionStateV10() {}

        protected AbstractInterviewExpressionStateV10(IDictionary<string, IExpressionExecutableV10> interviewScopes, Dictionary<string, List<string>> siblingRosters, IInterviewProperties interviewProperties)
            : this(interviewScopes.AsEnumerable(), siblingRosters, interviewProperties) {}

        protected AbstractInterviewExpressionStateV10(
            IEnumerable<KeyValuePair<string, IExpressionExecutableV10>> interviewScopes,
            Dictionary<string, List<string>> siblingRosters, IInterviewProperties interviewProperties)
            : base(
                interviewScopes
                    .ToDictionary<KeyValuePair<string, IExpressionExecutableV10>, string, IExpressionExecutableV9>(
                        item => item.Key,
                        item => item.Value),
                siblingRosters,
                interviewProperties)
        {
            this.RostersRemover = this.RemoveRosterAndItsDependencies;
            this.SetRosterRemoverAndChangesCollectorForAllScopes();
        }

        public override void AddRoster(Guid rosterId, 
            decimal[] outerRosterVector, 
            decimal rosterInstanceId,
            int? sortIndex)
        {
            base.AddRoster(rosterId, outerRosterVector, rosterInstanceId, sortIndex);

            decimal[] rosterVector = Util.GetRosterVector(outerRosterVector, rosterInstanceId);
            Guid[] rosterScopeIds = GetParentRosterScopeIds(rosterId);
            Identity[] rosterIdentityKey = Util.GetRosterKey(rosterScopeIds, rosterVector);
            string rosterStringKey = Util.GetRosterStringKey(rosterIdentityKey);

            if (this.InterviewScopes.ContainsKey(rosterStringKey))
            {
                var interviewScope = this.InterviewScopes[rosterStringKey];
                interviewScope.SetRostersRemover(this.RostersRemover);
                interviewScope.SetStructuralChangesCollector(this.StructuralChanges);
            }
        }

        public StructuralChanges StructuralChanges { get; set; } = new StructuralChanges();
      
        private IDictionary<string, IExpressionExecutableV10> interviewScopes;
        private Action<Identity[], Guid, decimal> RostersRemover;

        public new virtual IDictionary<string, IExpressionExecutableV10> InterviewScopes
            => this.interviewScopes ?? (this.interviewScopes = this.InitializeInterviewScopes());

        private IDictionary<string, IExpressionExecutableV10> InitializeInterviewScopes()
            => new TwoWayDictionaryAdapter<string, IExpressionExecutableV9, IExpressionExecutableV10>(
                () => base.InterviewScopes, ConvertExpressionExecutableV9ToV10, ConvertExpressionExecutableV10ToV9);

        private static IExpressionExecutableV9 ConvertExpressionExecutableV10ToV9(IExpressionExecutableV10 expressionExecutableV10)
            => expressionExecutableV10;

        private static IExpressionExecutableV10 ConvertExpressionExecutableV9ToV10(IExpressionExecutableV9 expressionExecutableV9)
        {
            if (expressionExecutableV9 == null)
                return null;

            if (expressionExecutableV9 is IExpressionExecutableV10)
                return (IExpressionExecutableV10) expressionExecutableV9;

            throw new NotSupportedException($"Interview scope expression executable V9 ({expressionExecutableV9.GetType().FullName}) cannot be converted to V10");
        }

        public new virtual IEnumerable<IExpressionExecutableV10> GetRosterInstances(Identity[] rosterKey, Guid scopeId)
            => base.GetRosterInstances(rosterKey, scopeId)?.Cast<IExpressionExecutableV10>();

        protected new virtual IExpressionExecutableV10 GetRosterByIdAndVector(Guid questionId, decimal[] rosterVector)
            => (IExpressionExecutableV10) base.GetRosterByIdAndVector(questionId, rosterVector);

        public IEnumerable<CategoricalOption> FilterOptionsForQuestion(Identity questionIdentity, IEnumerable<CategoricalOption> options)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionIdentity.Id, questionIdentity.RosterVector);
            if (targetLevel == null) return options;

            return targetLevel.FilterOptionsForQuestion(questionIdentity.Id, options);
        }

        public void RemoveAnswer(Identity questionIdentity)
        {
            IExpressionExecutableV10 targetLevel = this.GetRosterByIdAndVector(questionIdentity.Id, questionIdentity.RosterVector);
            targetLevel?.RemoveAnswer(questionIdentity.Id);
        }
    
        public new void SaveAllCurrentStatesAsPrevious()
        {
            base.SaveAllCurrentStatesAsPrevious();
            StructuralChanges.ClearAllChanges();
        }

        public virtual StructuralChanges GetStructuralChanges()
        {
            return this.StructuralChanges;
        }

        public new virtual EnablementChanges ProcessEnablementConditions()
        {
            var scopeKeys = new Queue<string>(this.InterviewScopes
                .OrderBy(x => x.Value.GetLevel())
                .Select(x => x.Key));

            List<EnablementChanges> enablementChangeses = new List<EnablementChanges>();
            while (scopeKeys.Count > 0)
            {
                var keyToProcess = scopeKeys.Dequeue();
                if (!this.InterviewScopes.ContainsKey(keyToProcess))
                    continue;

                var scope = this.InterviewScopes[keyToProcess];

                enablementChangeses.Add(scope.ProcessEnablementConditions());
            }

            return EnablementChanges.Union(enablementChangeses);
        }

        public override void RemoveRoster(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId)
        {
            if (!HasParentScropeRosterId(rosterId))
            {
                return;
            }

            decimal[] rosterVector = Util.GetRosterVector(outerRosterVector, rosterInstanceId);
            var rosterIdentityKey = Util.GetRosterKey(GetParentRosterScopeIds(rosterId), rosterVector);

            RemoveRoster(rosterIdentityKey);
        }

        public virtual void RemoveRoster(Identity[] rosterIdentityKey)
        {
            var rosterStringKey = Util.GetRosterStringKey((rosterIdentityKey));

            var dependentRosters = this.InterviewScopes.Keys.Where(x => x.StartsWith(rosterStringKey, StringComparison.Ordinal)).ToList();

            foreach (var rosterKey in dependentRosters)
            {
                var scope = this.InterviewScopes[rosterKey];
                if (scope != null)
                {
                    var deletedRosterIdentities = scope.GetRosterIdsThisScopeConsistOf().Select(x => new Identity(x, scope.RosterVector));
                    this.StructuralChanges.AddRemovedRosters(deletedRosterIdentities);
                }

                this.InterviewScopes.Remove(rosterKey);
                foreach (var siblings in this.SiblingRosters.Values)
                {
                    siblings.Remove(rosterKey);
                }
            }
        }

        public virtual void RemoveRosterAndItsDependencies(Identity[] rosterIdentities, Guid rosterSorceId, decimal rosterInstanceId)
        {
            var isQuestionnaireLevel = rosterIdentities.Length == 1 && rosterIdentities[0].Id == this.GetQuestionnaireId();

            var rosterKey = isQuestionnaireLevel ? new Identity[0] : rosterIdentities;
            var level = rosterKey.Length;

            var rosters = this.GetRosterInstances(rosterKey, rosterSorceId);

            if (rosters == null)
                return;

            var rostersKeysToDelete = rosters
                .Where(x => x.RosterVector[level] == rosterInstanceId)
                .Select(x => x.GetRosterKey())
                .ToList();

            foreach (var rosterKeyToDelete in rostersKeysToDelete)
            {
                this.RemoveRoster(rosterKeyToDelete);
            }
        }

        private void SetRosterRemoverAndChangesCollectorForAllScopes()
        {
            foreach (var interviewScope in this.InterviewScopes.Values)
            {
                interviewScope.SetRostersRemover(this.RostersRemover);
                interviewScope.SetStructuralChangesCollector(this.StructuralChanges);
            }
        }

        public override LinkedQuestionOptionsChanges ProcessLinkedQuestionFilters()
        {
            var result = new LinkedQuestionOptionsChanges();

            foreach (var scope in this.InterviewScopes.Values)
            {
                foreach (var linkedQuestionId in scope.LinkedQuestions)
                {
                    var linkedQuestionIdentity = new Identity(linkedQuestionId, scope.RosterVector);

                    var filteredResult = this.RunFiltersForLinkedQuestion(linkedQuestionId, scope, scope.RosterVector);

                    result.LinkedQuestionOptionsSet.Add(linkedQuestionIdentity, filteredResult);
                }
            }
            return result;
        }

        private RosterVector[] RunFiltersForLinkedQuestion(
            Guid linkedQuestionId, 
            IExpressionExecutableV10 linkedQuestionRosterScope, 
            RosterVector linkedQuestionRosterVector)
        {
            Identity[] linkedQuestionRosterScopeIds = linkedQuestionRosterScope.GetRosterKey();

            var scopesWithSourceQuestionsFiltereByLocation = this.InterviewScopes
                .Values
                .Where(x =>
                {
                    var rosterKey = x.GetRosterKey();
                    return this.DoesScopeWithSourceQuestionCorrespondToLinkedQuestion(
                        rosterKey,
                        rosterKey.Last().RosterVector,
                        linkedQuestionRosterScopeIds,
                        linkedQuestionRosterVector);
                });

            var filterResults = scopesWithSourceQuestionsFiltereByLocation
                .Select(scope => scope.ExecuteLinkedQuestionFilter(linkedQuestionRosterScope, linkedQuestionId))
                .Where(x => x?.Enabled ?? false)
                .Select(x => x.RosterKey.Last().RosterVector)
                .ToArray();

            return filterResults;
        }

        private bool DoesScopeWithSourceQuestionCorrespondToLinkedQuestion(
            Identity[] sourceRosterScopeIds, RosterVector sourceRosterVector, 
            Identity[] linkedRosterScopeIds, RosterVector linkedRosterVector)
        {
            var areLinkedAndSourceQuestionsOnSameLevel = SequenceEqualByIdentityId(linkedRosterScopeIds, sourceRosterScopeIds);
            if (areLinkedAndSourceQuestionsOnSameLevel)
            {
                if (sourceRosterVector.Length == linkedRosterVector.Length && linkedRosterVector.Length > 0)
                {
                    var linkedParentRosterVector = linkedRosterVector.Take(linkedRosterVector.Length - 1);
                    var sourceParentRosterVector = sourceRosterVector.Take(linkedRosterVector.Length - 1);

                    var doesScopesHasTheSameParent = sourceParentRosterVector.SequenceEqual(linkedParentRosterVector);
                    if (doesScopesHasTheSameParent)
                        return true;
                }
                else
                {
                    return true;
                }
            }

            int commonRosterLevel = this.GetCommonRosterLevel(linkedRosterScopeIds, sourceRosterScopeIds);
            var hasLinkedAndSourceQuestionsCommonParents = commonRosterLevel != 0;
            if (!hasLinkedAndSourceQuestionsCommonParents)
                return true;

            var isSourceQuestionDeeperThanLinkedQuestion = linkedRosterScopeIds.Length == commonRosterLevel;
            if (isSourceQuestionDeeperThanLinkedQuestion)
            {
                var sourceParentRosterVector = sourceRosterVector.Take(commonRosterLevel);
                if (!linkedRosterVector.SequenceEqual(sourceParentRosterVector))
                    return false;
            }

            var isLinkedQuestionDeeperThanSourceQuestion = sourceRosterScopeIds.Length == commonRosterLevel;
            if (isLinkedQuestionDeeperThanSourceQuestion)
            {
                var linkedParentRosterVector = linkedRosterVector.Take(commonRosterLevel - 1);
                var sourceParentRosterVector = sourceRosterVector.Take(commonRosterLevel - 1);

                var doesScopesHasTheSameParent = !sourceParentRosterVector.SequenceEqual(linkedParentRosterVector);
                if (doesScopesHasTheSameParent)
                    return false;
            }

            return true;
        }

        private static bool SequenceEqualByIdentityId(Identity[] source, Identity[] target)
        {
            if (source.Length == target.Length)
            {
                for (var i = 0; i < source.Length; i++)
                {
                    if (target[i].Id != source[i].Id)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        private int GetCommonRosterLevel(Identity[] rosterScopeIds1, Identity[] rosterScopeIds2)
        {
            int commonRosterLevel = 0;
            var minLength = Math.Min(rosterScopeIds1.Length, rosterScopeIds2.Length);
            for (int i = 0; i < minLength; i++)
            {
                if (rosterScopeIds1[i].Id == rosterScopeIds2[i].Id)
                {
                    commonRosterLevel++;
                }
                else
                {
                    break;
                }
            }
            return commonRosterLevel;
        }

        IInterviewExpressionStateV10 IInterviewExpressionStateV10.Clone() => (IInterviewExpressionStateV10) this.Clone();
    }
}