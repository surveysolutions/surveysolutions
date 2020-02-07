using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.Designer;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    class AttachmentsQuestionnaireImportStep : IQuestionnaireImportStep
    {
        private readonly QuestionnaireDocument questionnaire;
        private readonly Progress progress;
        private readonly IDesignerApi designerApi;
        private readonly IAttachmentContentService attachmentContentService;
        private readonly Dictionary<Attachment, RestFile> attachmentToProcess = new Dictionary<Attachment, RestFile>();

        public AttachmentsQuestionnaireImportStep(QuestionnaireDocument questionnaire, Progress progress, IDesignerApi designerApi, IAttachmentContentService attachmentContentService)
        {
            this.questionnaire = questionnaire;
            this.progress = progress;
            this.designerApi = designerApi;
            this.attachmentContentService = attachmentContentService;

            this.PrepareAttachmentsList();
        }

        public void PrepareAttachmentsList()
        {
            if (questionnaire.Attachments == null)
                return;

            foreach (var questionnaireAttachment in questionnaire.Attachments)
            {
                if (!attachmentContentService.HasAttachmentContent(questionnaireAttachment.ContentId))
                {
                    attachmentToProcess.Add(questionnaireAttachment, null);
                }
            }
        }

        public int GetPrecessStepsCount()
        {
            return attachmentToProcess.Count *  2;
        }

        public async Task DownloadFromDesignerAsync()
        {
            var questionnaireAttachments = attachmentToProcess.Keys.ToList();
            foreach (var questionnaireAttachment in questionnaireAttachments)
            {
                var attachmentContent = await designerApi.DownloadQuestionnaireAttachment(
                    questionnaireAttachment.ContentId, questionnaireAttachment.AttachmentId);
                attachmentToProcess[questionnaireAttachment] = attachmentContent;
                progress.Current++;
            }
        }

        public void SaveData()
        {
            foreach (var questionnaireAttachment in attachmentToProcess)
            {
                attachmentContentService.SaveAttachmentContent(
                    questionnaireAttachment.Key.ContentId,
                    questionnaireAttachment.Value.ContentType,
                    questionnaireAttachment.Value.FileName,
                    questionnaireAttachment.Value.Content);
                progress.Current++;
            }
        }
    }
}
