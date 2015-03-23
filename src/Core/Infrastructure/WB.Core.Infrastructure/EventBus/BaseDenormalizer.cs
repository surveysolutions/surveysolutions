namespace WB.Core.Infrastructure.EventBus
{
    public abstract class BaseDenormalizer : IEventHandler
    {
        public virtual string Name
        {
            get { return this.GetType().Name; }
        }

        public virtual object[] Readers
        {
            get { return new object[0]; }
        }
        
        public abstract object[] Writers { get; }
    }
}