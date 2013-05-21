namespace Main.DenormalizerStorage
{
    using System;
    using System.Linq;

    using Raven.Client;
    using Raven.Client.Document;

    #warning TLK: make string identifiers here after switch to new storage

    public class RavenDenormalizerStorage<TView> : IQueryableDenormalizerStorage<TView>
        where TView : class
    {
        private readonly DocumentStore ravenStore;

        public RavenDenormalizerStorage(DocumentStore ravenStore)
        {
            this.ravenStore = ravenStore;
        }

        public int Count()
        {
            using (var session = this.ravenStore.OpenSession())
            {
                return
                    session
                        .Query<TView>()
                        .Customize(customization
                            => customization.WaitForNonStaleResultsAsOfNow())
                        .Count();
            }
        }

        public TView GetById(Guid id)
        {
            using (var session = this.ravenStore.OpenSession())
            {
                return session.Load<TView>(id: id.ToString());
            }
        }

        public void Remove(Guid id)
        {
            using (var session = this.ravenStore.OpenSession())
            {
                var view = session.Load<TView>(id: id.ToString());

                session.Delete(view);
                session.SaveChanges();
            }
        }

        public void Store(TView view, Guid id)
        {
            using (var session = this.ravenStore.OpenSession())
            {
                session.Store(entity: view, id: id.ToString());
                session.SaveChanges();
            }
        }

        public IQueryable<TView> Query()
        {
            throw new NotImplementedException();
        }

        public TResult Query<TResult>(Func<IQueryable<TView>, TResult> query)
        {
            using (IDocumentSession session = this.ravenStore.OpenSession())
            {
                return query.Invoke(
                    session
                        .Query<TView>()
                        .Customize(customization
                            => customization.WaitForNonStaleResultsAsOfNow()));
            }
        }
    }
}