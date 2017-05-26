using System;

namespace WB.Core.BoundedContexts.Headquarters.Aggregates
{
    public class ReadonlyUser
    {
        public virtual Guid Id { get; protected set; }

        public virtual string Name { get; protected set; }

        public virtual ReadonlyProfile ReadonlyProfile { get; protected set; }
    }

    public class ReadonlyProfile
    {
        public virtual Guid? SupervisorId { get; protected set; }
        public virtual int Id { get; protected set; }
    }
}