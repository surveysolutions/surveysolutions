using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.V4;
using WB.Core.SharedKernels.DataCollection.V5;
using WB.Core.SharedKernels.DataCollection.V6;

namespace WB.Core.SharedKernels.DataCollection.V7
{
    public abstract class AbstractConditionalLevelInstanceV7<T> : AbstractConditionalLevelInstanceV6<T>
        where T : IExpressionExecutableV7, IExpressionExecutableV6, IExpressionExecutableV5, IExpressionExecutableV2, IExpressionExecutable
    {
        protected new Func<Identity[], Guid, IEnumerable<IExpressionExecutableV7>> GetInstances { get; private set; }

        protected new Dictionary<Guid, Func<decimal[], Identity[], IExpressionExecutableV7>> RosterGenerators { get;set; }

        protected Dictionary<Guid, Func<bool>> LinkedQuestionFilters = new Dictionary<Guid, Func<bool>>();

        protected AbstractConditionalLevelInstanceV7(decimal[] rosterVector, Identity[] rosterKey,
            Func<Identity[], Guid,
            IEnumerable<IExpressionExecutableV7>> getInstances, Dictionary<Guid, Guid[]> conditionalDependencies,
            Dictionary<Guid, Guid[]> structuralDependencies)
            : base(rosterVector, rosterKey, getInstances, conditionalDependencies, structuralDependencies)
        {
            this.GetInstances = getInstances;
            this.RosterGenerators = new Dictionary<Guid, Func<decimal[], Identity[], IExpressionExecutableV7>>();
        }

        protected AbstractConditionalLevelInstanceV7(decimal[] rosterVector, Identity[] rosterKey,
            Func<Identity[], Guid,
                IEnumerable<IExpressionExecutableV7>> getInstances, Dictionary<Guid, Guid[]> conditionalDependencies,
            Dictionary<Guid, Guid[]> structuralDependencies, IInterviewProperties properties)
            : this(rosterVector, rosterKey, getInstances, conditionalDependencies, structuralDependencies)
        {
            this.GetInstances = getInstances;
            this.Quest = properties;
        }

        public override IExpressionExecutableV6 CopyMembers(
           Func<Identity[], Guid, IEnumerable<IExpressionExecutableV6>> getInstances)
        {
            return null;
        }

        public abstract IExpressionExecutableV7 CopyMembers(
            Func<Identity[], Guid, IEnumerable<IExpressionExecutableV7>> getInstances);

        public new IExpressionExecutableV7 CreateChildRosterInstance(Guid rosterId, decimal[] rosterVector,
            Identity[] rosterIdentityKey)
        {
            return this.RosterGenerators[rosterId].Invoke(rosterVector, rosterIdentityKey);
        }

        public List<LinkedQuestionFilterResult> ExecuteLinkedQuestionFilters()
        {
            var result=new List<LinkedQuestionFilterResult>();

            var rosterStatesFromScope = this.EnablementStates.Where(r => this.RosterKey.Any(k => k.Id == r.Key)).Select(r=>r.Value.State).ToArray();

            if (rosterStatesFromScope.Length >0 && rosterStatesFromScope.All(s => s == State.Disabled))
                return result;

            foreach (var linkedQuestionFilter in this.LinkedQuestionFilters)
            {
                bool enabled = false;
                try
                {
                    enabled = linkedQuestionFilter.Value();
                }
#pragma warning disable
                catch (Exception ex)
                {
                }
#pragma warning restore
                result.Add(new LinkedQuestionFilterResult()
                {
                    Enabled = enabled,
                    LinkedQuestionId = linkedQuestionFilter.Key,
                    RosterKey = this.RosterKey
                });
            }
            return result;
        }
    }
}