﻿// -----------------------------------------------------------------------
// <copyright file="RavenPersistentStorage.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.Reflection;
using Newtonsoft.Json.Serialization;
using Raven.Abstractions.Data;
using Raven.Abstractions.Exceptions;
using Raven.Client;
using Raven.Client.Document;

namespace Main.DenormalizerStorage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class RavenPersistentStorage : IPersistentStorage
    {
        protected readonly IDocumentStore _documentStore;

        public RavenPersistentStorage(DocumentStore externalDocumentStore)
        {
            /*
              new EmbeddableDocumentStore { DataDirectory = externalDocumentStore.DataDirectory, UseEmbeddedHttpServer = false } : 
                    new DocumentStore { Url = externalDocumentStore.Url};*/
           /* var docStore = new DocumentStore {Url = externalDocumentStore.Url};
            docStore.Conventions = CreateConventions();
            docStore.Initialize();*/
            _documentStore = externalDocumentStore;       
        }

        #region Implementation of IPersistentStorage

        public T GetByGuid<T>(Guid key) where T : class
        {
            using (var session = _documentStore.OpenSession())
            {
                var obj = session.Load<StoredObject>(GetObjectId(typeof(T), key));
                if (obj != null)
                    return obj.Data as T;
            }
            return null;
        }

        public void Remove<T>(Guid key) where T : class
        {
            using (var session = _documentStore.OpenSession())
            {
                var obj = session.Load<StoredObject>(GetObjectId(typeof (T), key));
                if (obj != null)
                    session.Delete(obj);
            }
        }
        public void Store<T>(T denormalizer, Guid key) where T : class
        {
            Store(denormalizer, key, 0);
        }

        protected void Store<T>(T denormalizer, Guid key, int i) where T : class
        {
            using (var session = _documentStore.OpenSession())
            {
                session.Advanced.UseOptimisticConcurrency = true;
                var obj = session.Load<StoredObject>(GetObjectId(typeof (T), key));
                if (obj == null)
                {
                    obj = ToStoredObject(denormalizer, key);
                    session.Store(obj);
                }
                else
                {
                    obj.Data = denormalizer;
                }
                session.Advanced.GetMetadataFor(obj)[Constants.RavenEntityName] =
                    DocumentConvention.DefaultTypeTagName(denormalizer.GetType());
                try
                {
                    session.SaveChanges();
                }
                catch (ConcurrencyException ex)
                {
                    if (i < 3)
                        Store<T>(denormalizer, key, i + 1);
                    else
                    {
                        throw;
                    }
                }
            }
        }

        #endregion

        private StoredObject ToStoredObject<T>(T obj, Guid key) where T : class
        {
            return new StoredObject(obj, GetObjectId(obj.GetType(), key));
        }
        private string GetObjectId(Type objType, Guid key) 
        {
            return objType.Name + "/" + key.ToString();
        }
    }
}
