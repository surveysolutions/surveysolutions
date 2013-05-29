using WB.Core.Infrastructure;

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
        private readonly IReadLayerStatusService readLayerStatusService;

        public RavenDenormalizerStorage(DocumentStore ravenStore, IReadLayerStatusService readLayerStatusService)
        {
            this.ravenStore = ravenStore;
            this.readLayerStatusService = readLayerStatusService;
        }

        private static string ViewName
        {
            get { return typeof(TView).FullName; }
        }

        public int Count()
        {
            this.ThrowIfViewIsNotAccessible();

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
            this.ThrowIfViewIsNotAccessible();

            string ravenId = ToRavenId(id);

            using (var session = this.OpenSession())
            {
                return session.Load<TView>(id: ravenId);
            }
        }

        public void Remove(Guid id)
        {
            this.ThrowIfViewIsNotAccessible();

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
            this.ThrowIfViewIsNotAccessible();

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
            this.ThrowIfViewIsNotAccessible();

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
            this.ravenStore.DatabaseCommands.EnsureDatabaseExists("Views");
            return this.ravenStore.OpenSession("Views");
        }

        private static string ToRavenId(Guid id)
        {
            return string.Format("{0}:{1}", ViewName, id.ToString());
        }

        private void ThrowIfViewIsNotAccessible()
        {
            if (this.readLayerStatusService.AreViewsBeingRebuiltNow())
                throw new MaintenanceException("Views are currently being rebuilt. Therefore your request cannot be complete now.");
        }
    }
}