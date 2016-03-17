using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Repositories
{
    public interface IAttachmentContentStorage
    {
        Task StoreAsync(AttachmentContent attachmentContent);
        Task<AttachmentContentMetadata> GetMetadataAsync(string attachmentContentId);
        Task<bool> IsExistAsync(string attachmentContentId);
        Task<byte[]> GetContentAsync(string attachmentContentId);
    }
}