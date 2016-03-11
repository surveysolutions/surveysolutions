using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.QuestionnaireAttachments;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services
{
    public class QuestionnaireAttachmentService : IQuestionnaireAttachmentService
    {
        private readonly IPlainStorageAccessor<QuestionnaireAttachmentContent> attachmentContentStorage;
        private readonly IPlainStorageAccessor<QuestionnaireAttachment> attachmentStorage;
        private readonly IPlainTransactionManager transactionManager;

        public QuestionnaireAttachmentService(
            IPlainStorageAccessor<QuestionnaireAttachmentContent> attachmentContentStorage,
            IPlainStorageAccessor<QuestionnaireAttachment> attachmentStorage,
            IPlainTransactionManager transactionManager)
        {
            this.attachmentContentStorage = attachmentContentStorage;
            this.attachmentStorage = attachmentStorage;
            this.transactionManager = transactionManager;
        }

        public void SaveAttachment(QuestionnaireIdentity questionnaireIdentity, string md5OfAttachment,
            QuestionnaireAttachmentType type, string contentType, byte[] binaryContent)
        {
            this.transactionManager.ExecuteInPlainTransaction(() =>
            {
                if (this.attachmentContentStorage.GetById(md5OfAttachment) == null)
                {
                    this.attachmentContentStorage.Store(new QuestionnaireAttachmentContent
                    {
                        AttachmentId = md5OfAttachment,
                        Content = binaryContent
                    }, md5OfAttachment);
                }
                
                this.attachmentStorage.Store(new QuestionnaireAttachment
                {
                    AttachmentId = md5OfAttachment,
                    QuestionnairetIdentity = questionnaireIdentity,
                    AttachmentType = type,
                    AttachmentContentType = contentType
                }, Guid.NewGuid());
            });
        }

        public void DeleteAttachment(QuestionnaireIdentity questionnaireIdentity, string md5OfAttachment)
        {
            this.transactionManager.ExecuteInPlainTransaction(() =>
            {
                var attachmentsByMD5 = this.attachmentStorage.Query(attachments => attachments.Where(attachment => attachment.AttachmentId == md5OfAttachment));

                if (attachmentsByMD5.Count() == 1)
                    this.attachmentContentStorage.Remove(md5OfAttachment);

                foreach (var questionnaireAttachment in attachmentsByMD5.Where(attachment => attachment.QuestionnairetIdentity.Equals(questionnaireIdentity)))
                {
                    this.attachmentStorage.Remove(questionnaireAttachment.Id);
                }
            });
        }

        public byte[] GetAttachment(string md5OfAttachment)
        {
            return this.attachmentContentStorage.GetById(md5OfAttachment)?.Content;
        }

        public IEnumerable<QuestionnaireAttachment> GetAttachments(QuestionnaireIdentity questionnaireIdentity)
        {
            return this.attachmentStorage.Query(attachments =>
                attachments.Where(attachment => attachment.QuestionnairetIdentity.Equals(questionnaireIdentity)));
        }
    }
}
