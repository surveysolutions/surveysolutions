using System;
using System.Web.Security;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Shared.Web.Membership;
using WB.UI.Shared.Web.MembershipProvider.Accounts;

namespace WB.UI.Designer.Code.Implementation
{
    public class CommandPostprocessor : ICommandPostprocessor
    {
        private readonly IMembershipUserService userHelper;
        private readonly IRecipientNotifier notifier;
        private readonly IReadSideKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader;
        private readonly IAccountRepository accountRepository;
        private readonly ILogger logger;
        private readonly IAttachmentService attachmentService;
        private readonly ILookupTableService lookupTableService;
        private readonly ITranslationsService translationsService;


        public CommandPostprocessor(
            IMembershipUserService userHelper, 
            IRecipientNotifier notifier, 
            IAccountRepository accountRepository, 
            IReadSideKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader, 
            ILogger logger, 
            IAttachmentService attachmentService, 
            ILookupTableService lookupTableService, 
            ITranslationsService translationsService)
        {
            this.userHelper = userHelper;
            this.notifier = notifier;
            this.questionnaireDocumentReader = questionnaireDocumentReader;
            this.accountRepository = accountRepository;
            this.logger = logger;
            this.attachmentService = attachmentService;
            this.lookupTableService = lookupTableService;
            this.translationsService = translationsService;
        }


        public void ProcessCommandAfterExecution(ICommand command)
        {
            try
            {
                var addSharedPersonCommand = command as AddSharedPersonToQuestionnaire;
                if (addSharedPersonCommand != null)
                {
                    this.HandleNotifications(ShareChangeType.Share, addSharedPersonCommand.Email, addSharedPersonCommand.QuestionnaireId, addSharedPersonCommand.ShareType);
                    return;
                }

                var removeSharedPersonCommand = command as RemoveSharedPersonFromQuestionnaire;
                if (removeSharedPersonCommand != null)
                {
                    this.HandleNotifications(ShareChangeType.StopShare, removeSharedPersonCommand.Email, removeSharedPersonCommand.QuestionnaireId, ShareType.Edit);
                }

                var deleteAttachment = command as DeleteAttachment;
                if (deleteAttachment != null)
                {
                    attachmentService.Delete(deleteAttachment.AttachmentId);
                }

                var deleteLookupTable = command as DeleteLookupTable;
                if (deleteLookupTable != null)
                {
                    this.lookupTableService.DeleteLookupTableContent(deleteLookupTable.QuestionnaireId, deleteLookupTable.LookupTableId);
                }

                var deleteTranslation = command as DeleteTranslation;
                if (deleteTranslation != null)
                {
                    this.translationsService.Delete(deleteTranslation.QuestionnaireId, deleteTranslation.TranslationId);
                }

                var deleteQuestionnaire = command as DeleteQuestionnaire;
                if (deleteQuestionnaire != null)
                {
                    DeleteAccompanyingDataOnQuestionnaireRemove(deleteQuestionnaire.QuestionnaireId);
                }
            }
            catch (Exception exc)
            {
                logger.Error("Error on command post-processing", exc);
            }
        }

        private void DeleteAccompanyingDataOnQuestionnaireRemove(Guid questionnaireId)
        {
            var questionnaire = this.questionnaireDocumentReader.GetById(questionnaireId);

            foreach (var attachment in questionnaire.Attachments)
            {
                attachmentService.Delete(attachment.AttachmentId);
            }

            foreach (var lookupTable in questionnaire.LookupTables)
            {
                this.lookupTableService.DeleteLookupTableContent(questionnaireId, lookupTable.Key);
            }

            foreach (var translation in questionnaire.Translations)
            {
                this.translationsService.Delete(questionnaireId, translation.Id);
            }
        }

        private void HandleNotifications(ShareChangeType shareChangeType, string email, Guid questionnaireId, ShareType shareType)
        {
            var questionnaire = this.questionnaireDocumentReader.GetById(questionnaireId);

            string actionPersonEmail = this.userHelper.WebUser.MembershipUser.Email;
            string questionnaireTitle = questionnaire.Title;
            string personName = accountRepository.GetUserNameByEmail(email);

            this.notifier.NotifyTargetPersonAboutShareChange(shareChangeType, 
                email,
                personName,
                questionnaireId,
                questionnaireTitle,
                shareType,
                actionPersonEmail);

            if (questionnaire.CreatedBy.HasValue && questionnaire.CreatedBy.Value != this.userHelper.WebUser.UserId)
            {
                var user = accountRepository.GetByProviderKey(questionnaire.CreatedBy.Value);
                if (user != null)
                {
                    this.notifier.NotifyOwnerAboutShareChange(shareChangeType,
                        user.Email,
                        user.UserName,
                        questionnaireId,
                        questionnaireTitle,
                        shareType,
                        actionPersonEmail,
                        email);
                }
            }
        }
    }
}
