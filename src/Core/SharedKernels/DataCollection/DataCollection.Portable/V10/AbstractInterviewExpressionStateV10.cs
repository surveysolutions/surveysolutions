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
            this.SetRosterRemoverForAllScopes();
            this.SetAnswerAndStructureChangeNotifierForAllScopes();
        }

        public override void AddRoster(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId,
           int? sortIndex)
        {
            base.AddRoster(rosterId, outerRosterVector, rosterInstanceId, sortIndex);
            this.SetRosterRemoverForAllScopes();
            this.SetAnswerAndStructureChangeNotifierForAllScopes();
        }

        public StructuralChanges StructuralChanges { get; set; } = new StructuralChanges();
      

        private IDictionary<string, IExpressionExecutableV10> interviewScopes;

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
    
        public override void SaveAllCurrentStatesAsPrevious()
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
            var dependentRosters = this.InterviewScopes.Keys
                .Where(x => x.StartsWith(Util.GetRosterStringKey((rosterIdentityKey))))
                .ToArray();

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

        private void SetRosterRemoverForAllScopes()
        {
            foreach (var interviewScope in this.InterviewScopes.Values)
            {
                interviewScope.SetRostersRemover(this.RemoveRosterAndItsDependencies);
            }
        }

        private void SetAnswerAndStructureChangeNotifierForAllScopes()
        {
            foreach (var interviewScope in this.InterviewScopes.Values)
            {
                interviewScope.SetStructuralChangesCollector(this.StructuralChanges);
            }
        }

        public override LinkedQuestionOptionsChanges ProcessLinkedQuestionFilters()
        {
            var result = new LinkedQuestionOptionsChanges();

            foreach (var scope in this.InterviewScopes.Values)
            {
                foreach (var pair  in scope.LinkedQuestions)
                {
                    var linkedQuestionId = pair.Key;
                    var linkedQuestionIdentity = new Identity(linkedQuestionId, scope.RosterVector);

                    var filteredResult = GetRostersWithSourceQuestionsToRunFilter(linkedQuestionIdentity, scope);

                    result.LinkedQuestionOptionsSet.Add(linkedQuestionIdentity, filteredResult);
                }
            }

            /*
            var filterResults = new List<LinkedQuestionFilterResult>();
            foreach (var interviewScopeKvpValue in this.InterviewScopes.Values)
            {
                filterResults.AddRange(interviewScopeKvpValue.ExecuteLinkedQuestionFilters(interviewScopeKvpValue));
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
                if (!this.InterviewScopes.Values.Any(scope => scope.GetRosterKey().Any(r => r.Id == linkedQuestionIdWithSourceRosterIdPair.Value)))
                    result.LinkedQuestionOptions.Add(linkedQuestionIdWithSourceRosterIdPair.Key, new RosterVector[0]);
            }
            */
            return result;
        }

        private RosterVector[] GetRostersWithSourceQuestionsToRunFilter(Identity linkedQuestionIdentity, IExpressionExecutableV10 linkedQuestionRosterScope)
        {
            var filterResults = new List<LinkedQuestionFilterResult>();

            foreach (var scope in this.InterviewScopes.Values)
            {
                filterResults.AddRange(scope.ExecuteLinkedQuestionFilters(linkedQuestionRosterScope));
            }

            return filterResults
                .Where(x => x.Enabled)
                .Where(x => x.LinkedQuestionId == linkedQuestionIdentity.Id)
                .Select(x => new RosterVector(x.RosterKey.Last().RosterVector))
                .ToArray();
        }

        IInterviewExpressionStateV10 IInterviewExpressionStateV10.Clone() => (IInterviewExpressionStateV10) this.Clone();
    }
}