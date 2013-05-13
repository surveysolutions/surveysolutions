namespace Main.DenormalizerStorage
{
    using System;
    using System.Linq;

    using Raven.Client;
    using Raven.Client.Document;

    public class RavenDenormalizerStorage<TView> : IQueryableDenormalizerStorage<TView>
        where TView : class
    {
        private const int Timeout = 120;

        private readonly DocumentStore ravenStore;

        public RavenDenormalizerStorage(DocumentStore ravenStore)
        {
            this.ravenStore = ravenStore;
        }

        public int Count()
        {
            throw new NotImplementedException();
        }

        public TView GetById(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Remove(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Store(TView view, Guid id)
        {
            throw new NotImplementedException();
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
                            => customization.WaitForNonStaleResults(TimeSpan.FromSeconds(Timeout))));
            }
        }
    }
}