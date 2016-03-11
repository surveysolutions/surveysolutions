using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Views.QuestionnaireAttachments;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    public interface IQuestionnaireAttachmentService
    {
        void SaveAttachment(QuestionnaireIdentity questionnaireIdentity, string md5OfAttachment, QuestionnaireAttachmentType type,
            string contentType, byte[] binaryContent);
        byte[] GetAttachment(string md5OfAttachment);
        IEnumerable<QuestionnaireAttachment> GetAttachments(QuestionnaireIdentity questionnaireIdentity);
        void DeleteAttachment(QuestionnaireIdentity questionnaireIdentity, string md5OfAttachment);
    }
}