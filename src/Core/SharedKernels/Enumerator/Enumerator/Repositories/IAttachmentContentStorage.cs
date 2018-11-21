using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Api;

namespace WB.Core.SharedKernels.Enumerator.Repositories
{
    public interface IAttachmentContentStorage
    {
        void Store(AttachmentContent attachmentContent);
        AttachmentContentMetadata GetMetadata(string attachmentContentId);
        bool Exists(string attachmentContentId);
        byte[] GetContent(string attachmentContentId);
        string GetFileCacheLocation(string attachmentContentId);
        void Remove(string attachmentContentId);
        IEnumerable<string> EnumerateCache();
    }
}
