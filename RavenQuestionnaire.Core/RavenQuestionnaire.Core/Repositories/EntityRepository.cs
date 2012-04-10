using Raven.Client;
using RavenQuestionnaire.Core.Entities;

namespace RavenQuestionnaire.Core.Repositories
{
    public abstract class EntityRepository<TEntity, TDoc> : IEntityRepository<TEntity, TDoc> where TEntity : class, IEntity<TDoc>
        where TDoc : class 
    {
        private IDocumentSession documentSession;

        public EntityRepository(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        public TEntity Load(string id)
        {
            var document = this.documentSession.Load<TDoc>(id);
            if (document == null)
                return null;
            return Create(document);
        }

        public void Add(TEntity entity)
        {
            this.documentSession.Store(entity.GetInnerDocument());
        }

        public virtual void Remove(TEntity entity)
        {
            this.documentSession.Delete(entity.GetInnerDocument());
        }

        protected abstract TEntity Create(TDoc doc);
    }
}
