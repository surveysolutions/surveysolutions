using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface IAttachmentContentService
    {
        void SaveAttachmentContent(string contentHash, string contentType, string fileName, byte[] content);
        AttachmentContent GetAttachmentContent(string contentHash);
        void DeleteAttachmentContent(string contentHash);
        bool HasAttachmentContent(string contentHash);
        IEnumerable<AttachmentInfoView> GetAttachmentInfosByContentIds(IEnumerable<string> contentHashes);
    }
}
