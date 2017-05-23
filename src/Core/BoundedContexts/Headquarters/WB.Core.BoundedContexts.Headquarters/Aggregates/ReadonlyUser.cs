using System;

namespace WB.Core.BoundedContexts.Headquarters.Aggregates
{
    public class ReadonlyUser
    {
        public virtual Guid Id { get; protected set; }

        public virtual string Name { get; protected set; }
    }
}