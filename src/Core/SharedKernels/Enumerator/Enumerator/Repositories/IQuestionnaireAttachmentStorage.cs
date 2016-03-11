using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Repositories
{
    public interface IQuestionnaireAttachmentStorage
    {
        Task StoreAttachmentContentAsync(string attachmentId, byte[] attachmentData);
        Task StoreAsync(AttachmentMetadata attachmentMetadata);
        Task<AttachmentMetadata> GetAttachmentAsync(string attachmentId);
        Task<byte[]> GetAttachmentContentAsync(string attachmentId);
        Task<bool> IsExistAttachmentContentAsync(string attachmentId);
    }
}