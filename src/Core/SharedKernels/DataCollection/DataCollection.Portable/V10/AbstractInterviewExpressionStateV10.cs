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

        protected AbstractInterviewExpressionStateV10(IEnumerable<KeyValuePair<string, IExpressionExecutableV10>> interviewScopes, Dictionary<string, List<string>> siblingRosters, IInterviewProperties interviewProperties)
            : base(
                interviewScopes.ToDictionary<KeyValuePair<string, IExpressionExecutableV10>, string, IExpressionExecutableV9>(
                    item => item.Key,
                    item => item.Value),
                siblingRosters,
                interviewProperties) {}

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

        IInterviewExpressionStateV10 IInterviewExpressionStateV10.Clone() => (IInterviewExpressionStateV10) this.Clone();
    }
}