using System;

namespace WB.Core.Infrastructure.EventBus
{
    public abstract class BaseDenormalizer : IEventHandler
    {
        public virtual string Name
        {
            get { return this.GetType().FullName; }
        }

        public virtual Type[] UsesViews
        {
            get
            {
                return new Type[]{};
            }
        }

        public abstract Type[] BuildsViews { get; }
    }
}