using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Document;
using Raven.Imports.Newtonsoft.Json;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Utils;
using WB.Core.SharedKernel.Utils.Compression;
using WB.Core.SharedKernel.Utils.Serialization;

namespace WB.Core.Infrastructure.Raven.Implementation.ReadSide.RepositoryAccessors
{
    public class RavenReadSideRepositoryWriterWithCacheAndZip<TEntity> : IReadSideRepositoryWriter<TEntity>, IReadSideRepositoryReader<TEntity>
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly IReadSideRepositoryWriter<ZipView> internalImplementationOfWriter;
        private readonly IReadSideRepositoryReader<ZipView> internalImplementationOfReader;
        private readonly IStringCompressor compressor;
        private readonly Dictionary<Guid, TEntity> memcache; 
        private object locker = new object();

        private int memcacheItemsSizeLimit = 256; //avoid out of memory Exc

        public RavenReadSideRepositoryWriterWithCacheAndZip(
            IReadSideRepositoryWriter<ZipView> internalImplementationOfWriter,
            IReadSideRepositoryReader<ZipView> internalImplementationOfReader,
            IStringCompressor comperessor)
        {
            this.internalImplementationOfWriter = internalImplementationOfWriter;
            this.internalImplementationOfReader = internalImplementationOfReader;
            this.compressor = comperessor;
            this.memcache = new Dictionary<Guid, TEntity>();
            
         
        }

      /*  void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            if (previousViewId.HasValue)
                PersistItem(previousViewId.Value);
        }*/

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

        private Guid? previousViewId = null;

        public void Store(TEntity view, Guid id)
        {

            if (memcache.Count >= memcacheItemsSizeLimit)
            {
                if (previousViewId.HasValue)
                    PersistItem(previousViewId.Value);
                memcache.Clear();
            }

            memcache[id] = view;

            if (previousViewId.HasValue && previousViewId.Value != id)
                PersistItem(previousViewId.Value);
            previousViewId = id;
        }

        private void PersistItem(Guid id)
        {
            Task.Factory.StartNew(() =>
            {
                if (!memcache.ContainsKey(id))
                    return;
                var zipView = new ZipView(id, compressor.CompressObject(memcache[id]));
                internalImplementationOfWriter.Store(zipView, id);
            });
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