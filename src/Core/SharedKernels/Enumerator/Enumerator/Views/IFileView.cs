using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.SharedKernels.Enumerator.Views
{
    public interface IFileView : IPlainStorageEntity
    {
        string Id { get; set; }
        byte[] File { get; set; }
    }
}
