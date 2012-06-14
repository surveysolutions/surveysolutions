using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;
using Raven.Client.Linq;

namespace RavenQuestionnaire.Core
{
    public class CachableDocumentSessionAsync : IDocumentSession
    {
        private IAsyncDocumentSession _session;
        //private IAsyncDocumentSession _sessionAsync;
        private ConcurrentDictionary<string, object> _cache;

        private int SaveLimitCount = 3000;
        private readonly object syncObj = new object();
        private IDocumentStore _store;


        public CachableDocumentSessionAsync(IDocumentStore store, ConcurrentDictionary<string, object> cache)
        {
            //_sessionAsync = store.OpenAsyncSession();
            _store = store;
            _session = GetNewSession();
            _cache = cache;
        }
        

        public void Dispose()
        {
            
        }

        public void Delete<T>(T entity)
        {
            _session.Delete<T>(entity);
        }

        public T Load<T>(string id)
        {
            /*object temp;
            if (_cache.TryGetValue(id, out temp))
            {
                _sessionAsync.LoadAsync<T>(id);
                return (T)temp;
            }

            var loader = _sessionAsync.LoadAsync<T>(id);
            loader.Wait();

            T item = loader.Result;

            if (item != null)
                _cache.TryAdd(id, item);

            return item;*/
          

          /*  using (_session.Advanced.DocumentStore.AggressivelyCacheFor(TimeSpan.FromMinutes(5)))
            {
                return _session.Load<T>(id);
            }*/

            var loader = _session.LoadAsync<T>(id);
            loader.Wait();
            return loader.Result;


        }

        public T[] Load<T>(params string[] ids)
        {
            var loader = _session.LoadAsync<T>(ids);
            loader.Wait();
            return loader.Result;
        }

        public T[] Load<T>(IEnumerable<string> ids)
        {
            var loader = _session.LoadAsync<T>(ids.ToArray());
            loader.Wait();
            return loader.Result;

        }

        public T Load<T>(ValueType id)
        {
            var loader = _session.LoadAsync<T>(id);
            loader.Wait();
            return loader.Result;
         
        }

        public IRavenQueryable<T> Query<T>(string indexName)
        {
            return _session.Query<T>(indexName);
        }

        public IRavenQueryable<T> Query<T>()
        {
            return _session.Query<T>();
        }

        public IRavenQueryable<T> Query<T, TIndexCreator>() where TIndexCreator : AbstractIndexCreationTask, new()
        {
            throw new NotImplementedException();
            //return _session.Query<T, TIndexCreator>();
        }

        public ILoaderWithInclude<object> Include(string path)
        {
            throw new NotImplementedException();
            //return _session.Include(path);
        }

        public ILoaderWithInclude<T> Include<T>(Expression<Func<T, object>> path)
        {
            throw new NotImplementedException();
            //return _session.Include<T>(path);
        }

        public void SaveChanges()
        {
            lock (syncObj)
            {
                _session.SaveChangesAsync();

                if (_session.Advanced.NumberOfRequests >= SaveLimitCount - 30)
                {
                    _session = GetNewSession();
                }
            }
        }


        private IAsyncDocumentSession GetNewSession()
        {
            var session = _store.OpenAsyncSession();
            session.Advanced.MaxNumberOfRequestsPerSession = SaveLimitCount;
            return session;
        }

        public void Store(object entity, Guid etag)
        {
            _session.Store(entity, etag);
        }

        public void Store(object entity, Guid etag, string id)
        {
            throw new NotImplementedException();
            //_session.Store(entity, etag, id);
        }

        public void Store(dynamic entity)
        {
            _session.Store(entity);
        }

        public void Store(dynamic entity, string id)
        {
            throw new NotImplementedException();
            //_session.Store(entity,id);
        }

        public ISyncAdvancedSessionOperation Advanced
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
