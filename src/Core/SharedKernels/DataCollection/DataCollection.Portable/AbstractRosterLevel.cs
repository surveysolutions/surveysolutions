using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection
{
    public abstract class AbstractRosterLevel<T> : AbstractConditionalLevel<T> where T : IExpressionExecutable
    {
        protected AbstractRosterLevel(decimal[] rosterVector, Identity[] rosterKey, Func<Identity[], Guid, IEnumerable<IExpressionExecutable>> getInstances,
            Dictionary<Guid, Guid[]> conditionalDependencies, Dictionary<Guid, Guid[]> structureDependencies)
            : base(rosterVector, rosterKey, getInstances, conditionalDependencies, structureDependencies) { }
        
        public decimal index
        {
            get { return this.RosterVector.Last(); }
        }
    }
}