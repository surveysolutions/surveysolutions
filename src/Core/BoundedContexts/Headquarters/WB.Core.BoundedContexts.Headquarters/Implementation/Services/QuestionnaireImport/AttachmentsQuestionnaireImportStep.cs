using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.Designer;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    class AttachmentsQuestionnaireImportStep : IQuestionnaireImportStep
    {
        private readonly QuestionnaireDocument questionnaire;
        private readonly IDesignerApi designerApi;
        private readonly IAttachmentContentService attachmentContentService;
        private readonly Dictionary<Attachment, RestFile> attachmentToProcess = new Dictionary<Attachment, RestFile>();

        public AttachmentsQuestionnaireImportStep(QuestionnaireDocument questionnaire, IDesignerApi designerApi, IAttachmentContentService attachmentContentService)
        {
            this.questionnaire = questionnaire;
            this.designerApi = designerApi;
            this.attachmentContentService = attachmentContentService;

            this.PrepareAttachmentsList();
        }

        public void PrepareAttachmentsList()
        {
            if (questionnaire.Attachments != null && questionnaire.Attachments.Count > 0)
            {
                foreach (var questionnaireAttachment in questionnaire.Attachments)
                {
                    if (!attachmentContentService.HasAttachmentContent(questionnaireAttachment.ContentId))
                    {
                        attachmentToProcess.Add(questionnaireAttachment, null);
                    }
                }
            }
        }

        public bool IsNeedProcessing()
        {
            return attachmentToProcess.Count > 0;
        }

        public async Task DownloadFromDesignerAsync(IProgress<int> progress)
        {
            if (attachmentToProcess.Count > 0)
            {
                int percentPerAttachment = 100 / attachmentToProcess.Count;
                int currentPercent = 0;

                var questionnaireAttachments = attachmentToProcess.Keys.ToList();
                foreach (var questionnaireAttachment in questionnaireAttachments)
                {
                    var attachmentContent = await designerApi.DownloadQuestionnaireAttachment(
                        questionnaireAttachment.ContentId, questionnaireAttachment.AttachmentId);
                    attachmentToProcess[questionnaireAttachment] = attachmentContent;

                    currentPercent += percentPerAttachment;
                    progress.Report(currentPercent);
                }
            }
            progress.Report(100);
        }

        public void SaveData(IProgress<int> progress)
        {
            if (attachmentToProcess.Count > 0)
            {
                int percentPerAttachment = 100 / attachmentToProcess.Count;
                int currentPercent = 0;

                foreach (var questionnaireAttachment in attachmentToProcess)
                {
                    attachmentContentService.SaveAttachmentContent(
                        questionnaireAttachment.Key.ContentId,
                        questionnaireAttachment.Value.ContentType,
                        questionnaireAttachment.Value.FileName,
                        questionnaireAttachment.Value.Content);
                    currentPercent += percentPerAttachment;
                    progress.Report(currentPercent);
                }
            }

            progress.Report(100);
        }
    }
}
