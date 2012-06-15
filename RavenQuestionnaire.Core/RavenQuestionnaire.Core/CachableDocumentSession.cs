using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;
using Raven.Client.Linq;

namespace RavenQuestionnaire.Core
{
    public class CachableDocumentSession : IDocumentSession
    {
        private IDocumentSession _session;
        //private IAsyncDocumentSession _sessionAsync;
        private ConcurrentDictionary<string, object> _cache;

        private int SaveLimitCount =3000;
        private readonly object syncObj = new object();
        private IDocumentStore _store;


        public CachableDocumentSession(IDocumentStore store, ConcurrentDictionary<string, object> cache)
        {
            //_sessionAsync = store.OpenAsyncSession();
            _store = store;
            _session = GetNewSession();
            _cache = cache;
        }

        
        private IDocumentSession GetSession()
        {
            lock (syncObj)
            {
                return _session;
            }
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


            return _session.Load<T>(id);


        }

        public T[] Load<T>(params string[] ids)
        {
           return _session.Load<T>(ids);
        }

        public T[] Load<T>(IEnumerable<string> ids)
        {
        
            return _session.Load<T>(ids);
        }

        public T Load<T>(ValueType id)
        {
        
            return _session.Load<T>(id);
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
            return _session.Query<T, TIndexCreator>();
        }

        public ILoaderWithInclude<object> Include(string path)
        {
            return _session.Include(path);
        }

        public ILoaderWithInclude<T> Include<T>(Expression<Func<T, object>> path)
        {
            return _session.Include<T>(path);
        }

        public ILoaderWithInclude<T> Include<T, TInclude>(Expression<Func<T, object>> path)
        {
            return _session.Include(path);
        }

        public void SaveChanges()
        {
            lock (syncObj)
            {
                _session.SaveChanges();

                if (_session.Advanced.NumberOfRequests >= SaveLimitCount - 30)
                {
                    _session = GetNewSession();
                }
            }
        }


        private IDocumentSession GetNewSession()
        {
            var session = _store.OpenSession();
            session.Advanced.MaxNumberOfRequestsPerSession = SaveLimitCount;
            return session;
        }

        public void Store(object entity, Guid etag)
        {
            _session.Store(entity, etag);
        }

        public void Store(object entity, Guid etag, string id)
        {
            _session.Store(entity, etag, id);
        }

        public void Store(dynamic entity)
        {
            _session.Store(entity);
        }

        public void Store(dynamic entity, string id)
        {
            _session.Store(entity,id);
        }

        public ISyncAdvancedSessionOperation Advanced
        {
            get { return _session.Advanced; }
        }
    }
}
