using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Views.QuestionnaireAttachments
{
    public class QuestionnaireAttachment
    {
        public virtual Guid Id { get; set; }
        public virtual string AttachmentId { get; set; }
        public virtual QuestionnaireIdentity QuestionnairetIdentity { get; set; }
        public virtual QuestionnaireAttachmentType AttachmentType { get; set; }
        public virtual string AttachmentContentType { get; set; }
    }
}