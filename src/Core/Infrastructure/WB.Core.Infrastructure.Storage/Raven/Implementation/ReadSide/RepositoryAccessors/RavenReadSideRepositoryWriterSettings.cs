using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors
{
    public class RavenReadSideRepositoryWriterSettings
    {
        public RavenReadSideRepositoryWriterSettings(string basePath, int bulkInsertBatchSize)
        {
            this.BasePath = basePath;
            this.BulkInsertBatchSize = bulkInsertBatchSize;
        }

        public string BasePath { get; private set; }
        public int BulkInsertBatchSize { get; private set; }
    }
}
