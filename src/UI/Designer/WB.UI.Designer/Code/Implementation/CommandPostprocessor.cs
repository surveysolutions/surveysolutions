using System;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Shared.Web.Membership;
using WB.UI.Shared.Web.MembershipProvider.Accounts;

namespace WB.UI.Designer.Code.Implementation
{
    public class CommandPostprocessor : ICommandPostprocessor
    {
        private readonly IMembershipUserService userHelper;
        private readonly IRecipientNotifier notifier;
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader;
        private readonly IAccountRepository accountRepository;
        private readonly ILogger logger;
        private readonly IAttachmentService attachmentService;
        private readonly ILookupTableService lookupTableService;
        private readonly ITranslationsService translationsService;
        private readonly IPlainStorageAccessor<QuestionnaireListViewItem> questionnaireListViewItemStorage;
        private readonly IReadSideRepositoryWriter<AccountDocument> accountStorage;


        public CommandPostprocessor(
            IMembershipUserService userHelper, 
            IRecipientNotifier notifier, 
            IAccountRepository accountRepository,
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader, 
            ILogger logger, 
            IAttachmentService attachmentService, 
            ILookupTableService lookupTableService, 
            ITranslationsService translationsService,
            IPlainStorageAccessor<QuestionnaireListViewItem> questionnaireListViewItemStorage,
            IReadSideRepositoryWriter<AccountDocument> accountStorage)
        {
            this.userHelper = userHelper;
            this.notifier = notifier;
            this.questionnaireDocumentReader = questionnaireDocumentReader;
            this.accountRepository = accountRepository;
            this.logger = logger;
            this.attachmentService = attachmentService;
            this.lookupTableService = lookupTableService;
            this.translationsService = translationsService;
            this.questionnaireListViewItemStorage = questionnaireListViewItemStorage;
            this.accountStorage = accountStorage;
        }

        public void ProcessCommandAfterExecution(ICommand command)
        {
            var questionnaireCommand = command as QuestionnaireCommand;
            if (questionnaireCommand == null) return;

            try
            {
                this.UpdateListViewItem(questionnaireCommand);

                TypeSwitch.Do(command,
                    TypeSwitch.Case<ImportQuestionnaire>(cmd => this.CreateListViewItem(cmd.Source, true)),
                    TypeSwitch.Case<CloneQuestionnaire>(cmd => this.CreateListViewItem(cmd.Source, false)),
                    TypeSwitch.Case<CreateQuestionnaire>(this.CreateListViewItem),
                    TypeSwitch.Case<DeleteQuestionnaire>(x => this.DeleteAccompanyingDataOnQuestionnaireRemove(x.QuestionnaireId)),
                    TypeSwitch.Case<DeleteAttachment>(x => this.attachmentService.Delete(x.AttachmentId)),
                    TypeSwitch.Case<DeleteLookupTable>(x => this.lookupTableService.DeleteLookupTableContent(x.QuestionnaireId, x.LookupTableId)),
                    TypeSwitch.Case<DeleteTranslation>(x => this.translationsService.Delete(x.QuestionnaireId, x.TranslationId)));
            }
            catch (Exception exc)
            {
                logger.Error("Error on command post-processing", exc);
            }
        }

        private void CreateListViewItem(QuestionnaireDocument document, bool shouldPreserveSharedPersons)
        {
            var questionnaireListViewItem = new QuestionnaireListViewItem
            {
                PublicId = document.PublicKey,
                Title = document.Title,
                CreationDate = document.CreationDate,
                LastEntryDate = DateTime.UtcNow,
                CreatedBy = document.CreatedBy,
                IsPublic = document.IsPublic
            };

            if (document.CreatedBy.HasValue)
                questionnaireListViewItem.CreatorName = this.accountStorage.GetById(document.CreatedBy.Value)?.UserName;

            foreach (var sharedPerson in document.SharedPersons)
                questionnaireListViewItem.SharedPersons.Add(sharedPerson);

            if (shouldPreserveSharedPersons)
            {
                var sourceQuestionnaireListViewItem = this.questionnaireListViewItemStorage.GetById(questionnaireListViewItem.QuestionnaireId);
                if (sourceQuestionnaireListViewItem != null)
                {
                    foreach (var sharedPerson in sourceQuestionnaireListViewItem.SharedPersons)
                    {
                        if (!questionnaireListViewItem.SharedPersons.Contains(sharedPerson))
                        {
                            questionnaireListViewItem.SharedPersons.Add(sharedPerson);
                        }
                    }
                }
            }

            this.questionnaireListViewItemStorage.Store(questionnaireListViewItem, questionnaireListViewItem.QuestionnaireId);
        }

        private void CreateListViewItem(CreateQuestionnaire command)
        {;
            var questionnaireListViewItem = new QuestionnaireListViewItem
            {
                PublicId = command.QuestionnaireId,
                Title = command.Title,
                CreationDate = DateTime.UtcNow,
                LastEntryDate = DateTime.UtcNow,
                CreatedBy = command.ResponsibleId,
                IsPublic = command.IsPublic,
                CreatorName = this.accountStorage.GetById(command.ResponsibleId)?.UserName
            };
            this.questionnaireListViewItemStorage.Store(questionnaireListViewItem, questionnaireListViewItem.QuestionnaireId);
        }

        private void UpdateListViewItem(QuestionnaireCommand command)
        {
            var questionnaireId = command.QuestionnaireId.FormatGuid();
            var questionnaireListViewItem = this.questionnaireListViewItemStorage.GetById(questionnaireId);
            if (questionnaireListViewItem == null) return;

            TypeSwitch.Do(command,
                TypeSwitch.Case<AddSharedPersonToQuestionnaire>(cmd => this.AddSharedPerson(questionnaireListViewItem, cmd.PersonId, cmd.Email, cmd.ShareType)),
                TypeSwitch.Case<RemoveSharedPersonFromQuestionnaire>(cmd => this.RemoveSharedPerson(questionnaireListViewItem, cmd.PersonId, cmd.Email)),
                TypeSwitch.Case<DeleteQuestionnaire>(cmd => questionnaireListViewItem.IsDeleted = true),
                TypeSwitch.Case<UpdateQuestionnaire>(cmd =>
                {
                    questionnaireListViewItem.Title = cmd.Title;
                    questionnaireListViewItem.IsPublic = cmd.IsPublic;
                }));

            questionnaireListViewItem.LastEntryDate = DateTime.UtcNow;
            this.questionnaireListViewItemStorage.Store(questionnaireListViewItem, questionnaireId);
        }

        private void RemoveSharedPerson(QuestionnaireListViewItem questionnaireListViewItem, Guid personId, string personEmail)
        {
            if (questionnaireListViewItem.SharedPersons.Contains(personId))
                questionnaireListViewItem.SharedPersons.Remove(personId);

            this.HandleNotifications(ShareChangeType.StopShare, personEmail, questionnaireListViewItem.QuestionnaireId, ShareType.Edit);
        }

        private void AddSharedPerson(QuestionnaireListViewItem questionnaireListViewItem, Guid personId, string personEmail, ShareType shareType)
        {
            if (!questionnaireListViewItem.SharedPersons.Contains(personId))
                questionnaireListViewItem.SharedPersons.Add(personId);

            this.HandleNotifications(ShareChangeType.Share, personEmail, questionnaireListViewItem.QuestionnaireId, shareType);
        }

        private void DeleteAccompanyingDataOnQuestionnaireRemove(Guid questionnaireId)
        {
            var questionnaire = this.questionnaireDocumentReader.GetById(questionnaireId.FormatGuid());

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

        private void HandleNotifications(ShareChangeType shareChangeType, string email, string questionnaireId, ShareType shareType)
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
