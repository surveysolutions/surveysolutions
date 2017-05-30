using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NHibernate.Linq;
using WB.Core.Infrastructure.Fetching;

namespace WB.Infrastructure.Native.Fetching
{
    internal class NhFetchingProvider : IFetchingProvider
    {
        public IFetchRequest<TOriginating, TRelated> Fetch<TOriginating, TRelated>(IQueryable<TOriginating> query,
            Expression<Func<TOriginating, TRelated>> relatedObjectSelector)
        {
            var fetch = EagerFetchingExtensionMethods.Fetch(query, relatedObjectSelector);
            return new FetchRequest<TOriginating, TRelated>(fetch);
        }

        public IFetchRequest<TOriginating, TRelated> FetchMany<TOriginating, TRelated>(IQueryable<TOriginating> query,
            Expression<Func<TOriginating, IEnumerable<TRelated>>> relatedObjectSelector)
        {
            var fecth = EagerFetchingExtensionMethods.FetchMany(query, relatedObjectSelector);
            return new FetchRequest<TOriginating, TRelated>(fecth);
        }

        public IFetchRequest<TQueried, TRelated> ThenFetch<TQueried, TFetch, TRelated>(
            IFetchRequest<TQueried, TFetch> query, Expression<Func<TFetch, TRelated>> relatedObjectSelector)
        {
            var impl = query as FetchRequest<TQueried, TFetch>;
            var fetch = impl.NhFetchRequest.ThenFetch(relatedObjectSelector);
            return new FetchRequest<TQueried, TRelated>(fetch);
        }

        public IFetchRequest<TQueried, TRelated> ThenFetchMany<TQueried, TFetch, TRelated>(
            IFetchRequest<TQueried, TFetch> query,
            Expression<Func<TFetch, IEnumerable<TRelated>>> relatedObjectSelector)
        {
            var impl = query as FetchRequest<TQueried, TFetch>;
            var fetch = impl.NhFetchRequest.ThenFetchMany(relatedObjectSelector);
            return new FetchRequest<TQueried, TRelated>(fetch);
        }
    }
}