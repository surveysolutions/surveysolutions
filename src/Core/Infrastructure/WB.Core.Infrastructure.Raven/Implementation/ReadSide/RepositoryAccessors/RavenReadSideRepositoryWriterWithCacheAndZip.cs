using System;
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

        public RavenReadSideRepositoryWriterWithCacheAndZip(DocumentStore ravenStore,
                                                            IRavenReadSideRepositoryWriterRegistry writerRegistry,IReadSideStatusService readSideStatusService)
        {
            internalImplementationOfWriter = new RavenReadSideRepositoryWriter<ZipView>(ravenStore, writerRegistry);
            internalImplementationOfReader = new RavenReadSideRepositoryReader<ZipView>(ravenStore, readSideStatusService);
            compressor=new GZipJsonCompressor();
        }

        public int Count()
        {
            return internalImplementationOfReader.Count();
        }

        public TEntity GetById(Guid id)
        {
            var item = internalImplementationOfWriter.GetById(id);
            return compressor.DecompressString<TEntity>(item.Payload);
        }

        public void Remove(Guid id)
        {
            internalImplementationOfWriter.Remove(id);
        }

        public void Store(TEntity view, Guid id)
        {
            internalImplementationOfWriter.Store(new ZipView(id, compressor.CompressObject(view)), id);
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