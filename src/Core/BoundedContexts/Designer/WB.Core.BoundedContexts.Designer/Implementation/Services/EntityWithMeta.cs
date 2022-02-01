using Main.Core.Entities.Composite;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class EntityWithMeta
    {
        public EntityWithMeta(IComposite entity, EntityMeta meta)
        {
            Entity = entity;
            Meta = meta ;
        }

        public IComposite Entity { get; private set; }
        public EntityMeta Meta { get; private set; }
    }
}
