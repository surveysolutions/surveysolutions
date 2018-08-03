using SQLite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.SharedKernels.Enumerator.Views
{
    public class EnumeratorSettingsView : IPlainStorageEntity
    {
        [PrimaryKey]
        public string Id { get; set; }
        public string Endpoint { get; set; }
        public int HttpResponseTimeoutInSec { get; set; }
        public int CommunicationBufferSize { get; set; }
        public int? EventChunkSize { get; set; }
        public long? LastHqSyncTimestamp { get; set; }
    }
}
