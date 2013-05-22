namespace Main.DenormalizerStorage
{
    using System;
    using System.Linq;

    using Raven.Client;
    using Raven.Client.Document;

    using Raven.Client.Extensions;

    #warning TLK: make string identifiers here after switch to new storage
    public class RavenDenormalizerStorage<TView> : IQueryableDenormalizerStorage<TView>
        where TView : class
    {
        private readonly DocumentStore ravenStore;

        public RavenDenormalizerStorage(DocumentStore ravenStore)
        {
            this.ravenStore = ravenStore;
        }

        private static string ViewName
        {
            get { return typeof(TView).FullName; }
        }

        public int Count()
        {
            using (var session = this.OpenSession())
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
            string ravenId = ToRavenId(id);

            using (var session = this.OpenSession())
            {
                return session.Load<TView>(id: ravenId);
            }
        }

        public void Remove(Guid id)
        {
            string ravenId = ToRavenId(id);

            using (var session = this.OpenSession())
            {
                var view = session.Load<TView>(id: ravenId);

                session.Delete(view);
                session.SaveChanges();
            }
        }

        public void Store(TView view, Guid id)
        {
            string ravenId = ToRavenId(id);

            using (var session = this.OpenSession())
            {
                session.Store(entity: view, id: ravenId);
                session.SaveChanges();
            }
        }

        public IQueryable<TView> Query()
        {
            throw new NotImplementedException();
        }

        public TResult Query<TResult>(Func<IQueryable<TView>, TResult> query)
        {
            using (IDocumentSession session = this.OpenSession())
            {
                return query.Invoke(
                    session
                        .Query<TView>()
                        .Customize(customization
                            => customization.WaitForNonStaleResultsAsOfNow()));
            }
        }

        private IDocumentSession OpenSession()
        {
            return this.ravenStore.OpenSession();
        }

        private static string ToRavenId(Guid id)
        {
            return string.Format("{0}:{1}", ViewName, id.ToString());
        }
    }
}