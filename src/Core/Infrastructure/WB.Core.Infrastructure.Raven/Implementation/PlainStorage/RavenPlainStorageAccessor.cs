using Raven.Client;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Raven.PlainStorage;

namespace WB.Core.Infrastructure.Raven.Implementation.PlainStorage
{
    public class RavenPlainStorageAccessor<TEntity> : IPlainStorageAccessor<TEntity>
        where TEntity : class
    {
        private readonly IRavenPlainStorageProvider storageProvider;

        public RavenPlainStorageAccessor(IRavenPlainStorageProvider storageProvider)
        {
            this.storageProvider = storageProvider;
        }

        private static string EntityName
        {
            get { return typeof (TEntity).Name; }
        }

        public TEntity GetById(string id)
        {
            using (IDocumentSession session = this.OpenSession())
            {
                string ravenId = ToRavenId(id);

                var entity = session.Load<TEntity>(id: ravenId);

                return entity;
            }
        }

        public void Remove(string id)
        {
            using (IDocumentSession session = this.OpenSession())
            {
                string ravenId = ToRavenId(id);

                var entity = session.Load<TEntity>(id: ravenId);

                if (entity == null)
                    return;

                session.Delete(entity);
                session.SaveChanges();
            }
        }

        public void Store(TEntity entity, string id)
        {
            using (IDocumentSession session = this.OpenSession())
            {
                string ravenId = ToRavenId(id);

                session.Store(entity: entity, id: ravenId);
                session.SaveChanges();
            }
        }

        private IDocumentSession OpenSession()
        {
            return this.storageProvider.GetDocumentStore().OpenSession();
        }

        protected static string ToRavenId(string id)
        {
            return string.Format("{0}${1}", EntityName, id);
        }
    }
}