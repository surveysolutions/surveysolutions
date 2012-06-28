using System;
using System.Collections.Concurrent;
using System.Linq;

namespace RavenQuestionnaire.Core.Denormalizers
{
    public interface IDenormalizerStorage<T> where T : class
    {
        T GetByGuid(Guid key);
        IQueryable<T> Query();
        void Store(T denormalizer, Guid key) ;
      /*  void Commit();*/
    }

    public class RavenSessionDenormalizer<T> : IDenormalizerStorage<T> where T : class
    {
      //  private IDocumentSession documentSession;
        private ConcurrentDictionary<Guid, T> hash;
        public RavenSessionDenormalizer(/*IDocumentSession documentSession*/)
        {
            //this.documentSession = documentSession;
            this.hash=new ConcurrentDictionary<Guid, T>();
        }

        #region Implementation of IDenormalizerStorage

        public T GetByGuid(Guid key) 
        {
            if (!this.hash.ContainsKey(key))
                return null;
            return this.hash[key];
            //  return this.documentSession.Load<T>(key.ToString());
        }

        public IQueryable<T> Query()
        {
            return this.hash.Values.AsQueryable();
            // return this.documentSession.Query<T>();
        }

        public void Store(T denormalizer, Guid key)
        {
            if (this.hash.ContainsKey(key))
            {

                hash[key] = denormalizer;
                return;
            }
            hash.TryAdd(key, denormalizer);
            //  this.documentSession.Store(denormalizer);
        }
/*
        public void Commit()
        {
            this.documentSession.SaveChanges();
        }*/

        #endregion
    }
}
