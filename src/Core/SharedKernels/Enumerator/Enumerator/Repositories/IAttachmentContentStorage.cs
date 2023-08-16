using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Api;

namespace WB.Core.SharedKernels.Enumerator.Repositories
{
    public interface IAttachmentContentStorage
    {
        Task StoreAsync(AttachmentContent attachmentContent);
        AttachmentContentMetadata GetMetadata(string attachmentContentId);
        Task<bool> ExistsAsync(string attachmentContentId);
        Task<byte[]> GetContentAsync(string attachmentContentId);
        Task<byte[]> GetPreviewContentAsync(string attachmentContentId);
        Task<string> GetFileCacheLocationAsync(string attachmentContentId);
        Task RemoveAsync(string attachmentContentId);
        Task<IEnumerable<string>> EnumerateCacheAsync();
    }
}
