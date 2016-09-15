using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Repositories
{
    public interface IAttachmentContentStorage
    {
        void Store(AttachmentContent attachmentContent);
        AttachmentContentMetadata GetMetadata(string attachmentContentId);
        bool Exists(string attachmentContentId);
        byte[] GetContent(string attachmentContentId);
    }
}