using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories
{
    public class QuestionnaireAttachmentStorage : IQuestionnaireAttachmentStorage
    {
        public void Store(Attachment attachment, byte[] attachmentData)
        {
            throw new NotImplementedException();
        }

        public Task<Attachment> GetAttachmentAsync(string attachmentId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Attachment>> GetAttachmentsByQuestionnaireAsync(Guid questionnaireId)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GetAttachmentContentAsync(string attachmentId)
        {
            throw new NotImplementedException();
        }
    }
}