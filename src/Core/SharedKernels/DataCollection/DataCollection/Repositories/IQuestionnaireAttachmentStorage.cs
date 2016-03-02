using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    public interface IQuestionnaireAttachmentStorage
    {
        void Store(Attachment attachment, byte[] attachmentData);
        Task<Attachment> GetAttachmentAsync(string attachmentId);
        Task<IEnumerable<Attachment>> GetAttachmentsByQuestionnaireAsync(Guid questionnaireId);
        Task<byte[]> GetAttachmentContentAsync(string attachmentId);
    }
}