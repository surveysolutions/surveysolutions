using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Raven.Client.Document;
using Raven.Imports.Newtonsoft.Json;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Utils;
using WB.Core.SharedKernel.Utils.Compression;

namespace WB.Core.Infrastructure.Raven.Implementation.ReadSide.RepositoryAccessors
{
    public class RavenReadSideRepositoryWriterWithCacheAndZip<TEntity> : IReadSideRepositoryWriter<TEntity>, IReadSideRepositoryReader<TEntity>
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly RavenReadSideRepositoryWriter<ZipView> internalImplementationOfWriter;
        private readonly RavenReadSideRepositoryReader<ZipView> internalImplementationOfReader;
        private readonly GZipJsonCompressor compressor;
        private readonly Dictionary<Guid, TEntity> memcache; 
        private object locker = new object();

        private int memcacheItemsSizeLimit = 256; //avoid memory

        public RavenReadSideRepositoryWriterWithCacheAndZip(DocumentStore ravenStore,
                                                            IRavenReadSideRepositoryWriterRegistry writerRegistry,IReadSideStatusService readSideStatusService)
        {
            internalImplementationOfWriter = new RavenReadSideRepositoryWriter<ZipView>(ravenStore, writerRegistry);
            internalImplementationOfReader = new RavenReadSideRepositoryReader<ZipView>(ravenStore, readSideStatusService);
            compressor=new GZipJsonCompressor();
            memcache = new Dictionary<Guid, TEntity>();
        }

        public int Count()
        {
            return internalImplementationOfReader.Count();
        }

        public TEntity GetById(Guid id)
        {
            if (!memcache.ContainsKey(id))
            {
                lock (locker)
                {
                    if (!memcache.ContainsKey(id))
                    {
                        var view = compressor.DecompressString<TEntity>(
                            internalImplementationOfWriter.GetById(id).Payload);
                        memcache.Add(id,view);
                    }
                }
            }
            try
            {
                return memcache[id];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
           
        }

        public void Remove(Guid id)
        {
            lock (locker)
            {
                internalImplementationOfWriter.Remove(id);
                memcache.Remove(id);
            }
        }

        public void Store(TEntity view, Guid id)
        {
            lock (locker)
            {
                if (memcache.Count >= memcacheItemsSizeLimit)
                    memcache.Clear();

                    internalImplementationOfWriter.Store(new ZipView(id, compressor.CompressObject(view)), id);
                memcache[id] = view;
            }
        }
      
    }
    public class ZipView : IView
    {
        public ZipView(Guid publicKey, string payload)
        {
            PublicKey = publicKey;
            Payload = payload;
        }

        public Guid PublicKey { get; private set; }
        public string Payload { get; private set; }
    }
}