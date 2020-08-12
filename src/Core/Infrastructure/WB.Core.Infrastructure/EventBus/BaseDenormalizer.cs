namespace WB.Core.Infrastructure.EventBus
{
    public abstract class BaseDenormalizer : IEventHandler
    {
        public virtual string Name
        {
            get { return this.GetType().Name; }
        }
    }
}
