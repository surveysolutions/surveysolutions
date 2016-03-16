using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Repositories
{
    public interface IAttachmentContentStorage
    {
        Task StoreAttachmentContentAsync(AttachmentContent attachmentContent);
        Task<AttachmentContent> GetAttachmentContentAsync(string attachmentContentId);
        Task<bool> IsExistAttachmentContentAsync(string attachmentContentId);
    }
}