using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    public interface IQuestionnaireAttachmentStorage
    {
        Task StoreAsync(Attachment attachment, byte[] attachmentData);
        Task<Attachment> GetAttachmentAsync(string attachmentId);
        Task<byte[]> GetAttachmentContentAsync(string attachmentId);
    }
}