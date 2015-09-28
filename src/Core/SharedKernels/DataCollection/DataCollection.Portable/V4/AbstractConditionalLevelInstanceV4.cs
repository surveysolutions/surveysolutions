using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.V3.CustomFunctions;

namespace WB.Core.SharedKernels.DataCollection.V4
{
    public abstract class AbstractConditionalLevelInstanceV4<T> : AbstractConditionalLevelInstanceV3<T> where T : IExpressionExecutableV2
    {
        protected new Func<Identity[], Guid, IEnumerable<IExpressionExecutableV2>> GetInstances { get; private set; }
        protected new Dictionary<Guid, Func<decimal[], Identity[], IExpressionExecutableV2>> RosterGenerators { get; set; }

        protected AbstractConditionalLevelInstanceV4(decimal[] rosterVector, Identity[] rosterKey, Func<Identity[], Guid, 
            IEnumerable<IExpressionExecutableV2>> getInstances, Dictionary<Guid, Guid[]> conditionalDependencies, 
            Dictionary<Guid, Guid[]> structuralDependencies)
            : base(rosterVector, rosterKey, getInstances, conditionalDependencies, structuralDependencies)
        {
            GetInstances = getInstances;
            this.RosterGenerators = new Dictionary<Guid, Func<decimal[], Identity[], IExpressionExecutableV2>>();
        }

        protected AbstractConditionalLevelInstanceV4(decimal[] rosterVector, Identity[] rosterKey, Func<Identity[], Guid,
            IEnumerable<IExpressionExecutableV2>> getInstances, Dictionary<Guid, Guid[]> conditionalDependencies,
            Dictionary<Guid, Guid[]> structuralDependencies, IInterviewProperties properties)
            : this(rosterVector, rosterKey, getInstances, conditionalDependencies, structuralDependencies)
        {
            GetInstances = getInstances;
            this.Quest = properties;
        }

        public void SetInterviewProperties(IInterviewProperties properties)
        {
            this.Quest = properties;
        }

        protected IInterviewProperties Quest { get; set; }


        public abstract IExpressionExecutableV2 CopyMembers(Func<Identity[], Guid, IEnumerable<IExpressionExecutableV2>> getInstances);

        public new IExpressionExecutableV2 CreateChildRosterInstance(Guid rosterId, decimal[] rosterVector, Identity[] rosterIdentityKey)
        {
            return this.RosterGenerators[rosterId].Invoke(rosterVector, rosterIdentityKey);
        }
    }
}