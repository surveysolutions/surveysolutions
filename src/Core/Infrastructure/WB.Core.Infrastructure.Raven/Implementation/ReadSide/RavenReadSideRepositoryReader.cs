using System;
using System.Linq;

using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Extensions;

using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.Infrastructure.Raven.Implementation.ReadSide
{
    #warning TLK: make string identifiers here after switch to new storage
    public class RavenReadSideRepositoryReader<TEntity> : RavenReadSideRepositoryAccessor<TEntity>, IQueryableReadSideRepositoryReader<TEntity>
        where TEntity : class, IReadSideRepositoryEntity
    {
        public RavenReadSideRepositoryReader(DocumentStore ravenStore)
            : base(ravenStore) { }

        public int Count()
        {
            using (var session = this.OpenSession())
            {
                return
                    session
                        .Query<TEntity>()
                        .Customize(customization
                            => customization.WaitForNonStaleResultsAsOfNow())
                        .Count();
            }
        }

        public TEntity GetById(Guid id)
        {
            string ravenId = ToRavenId(id);

            using (var session = this.OpenSession())
            {
                return session.Load<TEntity>(id: ravenId);
            }
        }

        public TResult Query<TResult>(Func<IQueryable<TEntity>, TResult> query)
        {
            using (IDocumentSession session = this.OpenSession())
            {
                return query.Invoke(
                    session
                        .Query<TEntity>()
                        .Customize(customization
                            => customization.WaitForNonStaleResultsAsOfNow()));
            }
        }
    }
}