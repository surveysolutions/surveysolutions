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
        bool Exists(string attachmentContentId);
        byte[] GetContent(string attachmentContentId);
        byte[] GetPreviewContent(string attachmentContentId);
        string GetFileCacheLocation(string attachmentContentId);
        void Remove(string attachmentContentId);
        Task<IEnumerable<string>> EnumerateCacheAsync();
    }
}
