using SQLite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.SharedKernels.Enumerator.Views
{
    public class CompanyLogo : IPlainStorageEntity
    {
        [PrimaryKey]
        public string Id { get; set; }
        
        public byte[] File { get; set; }

        public string ETag { get; set; }

        public const string StorageKey = "logo";
    }
}