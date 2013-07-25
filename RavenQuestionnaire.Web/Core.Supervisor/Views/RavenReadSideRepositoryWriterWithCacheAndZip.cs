using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core;
using Newtonsoft.Json;
using Raven.Client.Document;
using WB.Core.Infrastructure.Raven.Implementation.ReadSide;
using WB.Core.Infrastructure.Raven.Implementation.ReadSide.RepositoryAccessors;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Core.Supervisor.Views
{
    public class RavenReadSideRepositoryWriterWithCacheAndZip<TEntity> : IReadSideRepositoryWriter<TEntity>, IReadSideRepositoryReader<TEntity>
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly RavenReadSideRepositoryWriter<ZipView> internalImplementationOfWriter;
        private readonly RavenReadSideRepositoryReader<ZipView> internalImplementationOfReader;

        public RavenReadSideRepositoryWriterWithCacheAndZip(DocumentStore ravenStore,
                                                            IRavenReadSideRepositoryWriterRegistry writerRegistry,IReadSideStatusService readSideStatusService)
        {
            internalImplementationOfWriter = new RavenReadSideRepositoryWriter<ZipView>(ravenStore, writerRegistry);
            internalImplementationOfReader = new RavenReadSideRepositoryReader<ZipView>(ravenStore, readSideStatusService);
          //  internalImplementationOfReader.EnableCache();
        }

        public int Count()
        {
            return internalImplementationOfReader.Count();
        }

        public TEntity GetById(Guid id)
        {
            var zipView = PackageHelper.DecompressString(internalImplementationOfWriter.GetById(id).Payload);
            return JsonConvert.DeserializeObject<TEntity>(zipView, JsonSerializerSettings);
        }

        public void Remove(Guid id)
        {
            internalImplementationOfWriter.Remove(id);
        }

        public void Store(TEntity view, Guid id)
        {
            var viewToString = GetItemAsContent(view);
            internalImplementationOfWriter.Store(new ZipView(id, PackageHelper.CompressString(viewToString)), id);
        }
        private string GetItemAsContent(object item)
        {
            return JsonConvert.SerializeObject(item, Formatting.None, JsonSerializerSettings);
        }

        private JsonSerializerSettings JsonSerializerSettings
        {
            get
            {
                return  new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Objects,
                        NullValueHandling = NullValueHandling.Ignore
                    };
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

        public Guid PublicKey { get;private set; }
        public string Payload { get;private set; }
    }
}
