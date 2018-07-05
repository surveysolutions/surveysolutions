using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Variable;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Services.Accounts;
using WB.Core.BoundedContexts.Designer.Views;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.QuestionnairePostProcessors
{
    internal class ListViewPostProcessor : 
        ICommandPostProcessor<Questionnaire, ImportQuestionnaire>,
        ICommandPostProcessor<Questionnaire, CloneQuestionnaire>,
        ICommandPostProcessor<Questionnaire, CreateQuestionnaire>,
        ICommandPostProcessor<Questionnaire, UpdateQuestionnaire>,
        ICommandPostProcessor<Questionnaire, DeleteQuestionnaire>,
        ICommandPostProcessor<Questionnaire, AddSharedPersonToQuestionnaire>,
        ICommandPostProcessor<Questionnaire, RemoveSharedPersonFromQuestionnaire>,
        ICommandPostProcessor<Questionnaire, AddStaticText>,
        ICommandPostProcessor<Questionnaire, UpdateStaticText>,
        ICommandPostProcessor<Questionnaire, MoveStaticText>,
        ICommandPostProcessor<Questionnaire, DeleteStaticText>,
        ICommandPostProcessor<Questionnaire, AddVariable>,
        ICommandPostProcessor<Questionnaire, UpdateVariable>,
        ICommandPostProcessor<Questionnaire, MoveVariable>,
        ICommandPostProcessor<Questionnaire, DeleteVariable>,
        ICommandPostProcessor<Questionnaire, AddMacro>,
        ICommandPostProcessor<Questionnaire, UpdateMacro>,
        ICommandPostProcessor<Questionnaire, DeleteMacro>,
        ICommandPostProcessor<Questionnaire, AddOrUpdateAttachment>,
        ICommandPostProcessor<Questionnaire, DeleteAttachment>,
        ICommandPostProcessor<Questionnaire, AddOrUpdateTranslation>,
        ICommandPostProcessor<Questionnaire, DeleteTranslation>,
        ICommandPostProcessor<Questionnaire, SetDefaultTranslation>,
        ICommandPostProcessor<Questionnaire, AddGroup>,
        ICommandPostProcessor<Questionnaire, UpdateGroup>,
        ICommandPostProcessor<Questionnaire, MoveGroup>,
        ICommandPostProcessor<Questionnaire, DeleteGroup>,
        ICommandPostProcessor<Questionnaire, PasteAfter>,
        ICommandPostProcessor<Questionnaire, PasteInto>,
        ICommandPostProcessor<Questionnaire, AddDefaultTypeQuestion>,
        ICommandPostProcessor<Questionnaire, DeleteQuestion>,
        ICommandPostProcessor<Questionnaire, MoveQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateMultimediaQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateDateTimeQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateNumericQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateQRBarcodeQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateGpsCoordinatesQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateTextListQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateTextQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateMultiOptionQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateSingleOptionQuestion>,
        ICommandPostProcessor<Questionnaire, AddLookupTable>,
        ICommandPostProcessor<Questionnaire, UpdateLookupTable>,
        ICommandPostProcessor<Questionnaire, DeleteLookupTable>,
        ICommandPostProcessor<Questionnaire, UpdateCascadingComboboxOptions>,
        ICommandPostProcessor<Questionnaire, UpdateFilteredComboboxOptions>,
        ICommandPostProcessor<Questionnaire, RevertVersionQuestionnaire>,
        ICommandPostProcessor<Questionnaire, UpdateAreaQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateAudioQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateMetadata>
    {
        private IPlainStorageAccessor<User> accountStorage
            => ServiceLocator.Current.GetInstance<IPlainStorageAccessor<User>>();

        private IPlainStorageAccessor<QuestionnaireListViewItem> questionnaireListViewItemStorage
            => ServiceLocator.Current.GetInstance<IPlainStorageAccessor<QuestionnaireListViewItem>>();

        private IRecipientNotifier emailNotifier => ServiceLocator.Current.GetInstance<IRecipientNotifier>();

        private IAccountRepository accountRepository => ServiceLocator.Current.GetInstance<IAccountRepository>();

        private void Create(QuestionnaireDocument document, bool shouldPreserveSharedPersons, 
            Guid? questionnaireId = null, string questionnaireTitle = null, Guid? createdBy = null,
            bool? isPublic = null, DateTime? creationDate = null)
        {
            var questionnaireIdValue = questionnaireId ?? document.PublicKey;

            var creatorId = createdBy ?? document.CreatedBy;

            var sourceQuestionnaireListViewItem = this.questionnaireListViewItemStorage.GetById(questionnaireIdValue.FormatGuid());
            var questionnaireListViewItem = sourceQuestionnaireListViewItem ?? new QuestionnaireListViewItem();
            questionnaireListViewItem.PublicId = questionnaireIdValue;
            questionnaireListViewItem.Title = questionnaireTitle ?? document.Title;
            questionnaireListViewItem.CreationDate = creationDate ?? document.CreationDate;
            questionnaireListViewItem.LastEntryDate = DateTime.UtcNow;
            questionnaireListViewItem.CreatedBy = creatorId;
            questionnaireListViewItem.IsPublic = isPublic ?? document.IsPublic;
            questionnaireListViewItem.Owner = null;
            questionnaireListViewItem.CreatorName = null;
            questionnaireListViewItem.IsDeleted = false;

            if (creatorId.HasValue)
                questionnaireListViewItem.CreatorName = this.accountStorage.GetById(creatorId.Value.FormatGuid())?.UserName;

            if (!shouldPreserveSharedPersons)
            {
                questionnaireListViewItem.SharedPersons.Clear();
            }
            else
            {
                if (creatorId.HasValue)
                {
                    questionnaireListViewItem.SharedPersons.Where(p => p.IsOwner && p.UserId != creatorId).ForEach(p => p.IsOwner = false);
                    var owner = questionnaireListViewItem.SharedPersons.FirstOrDefault(p => p.UserId == creatorId);
                    if (owner != null)
                    {
                        owner.IsOwner = true;
                        owner.ShareType = ShareType.Edit;
                    }
                }
            }

            this.questionnaireListViewItemStorage.Store(questionnaireListViewItem, questionnaireListViewItem.QuestionnaireId);
        }

        private void Create(CreateQuestionnaire command)
        {
            var questionnaireListViewItem = new QuestionnaireListViewItem
            {
                PublicId = command.QuestionnaireId,
                Title = command.Title,
                CreationDate = DateTime.UtcNow,
                LastEntryDate = DateTime.UtcNow,
                CreatedBy = command.ResponsibleId,
                IsPublic = command.IsPublic,
                CreatorName = this.accountStorage.GetById(command.ResponsibleId.FormatGuid())?.UserName
            };
            this.questionnaireListViewItemStorage.Store(questionnaireListViewItem, questionnaireListViewItem.QuestionnaireId);
        }

        private void Update(string questionnaireId)
        {
            var questionnaireListViewItem = this.questionnaireListViewItemStorage.GetById(questionnaireId);
            if (questionnaireListViewItem == null) return;

            questionnaireListViewItem.LastEntryDate = DateTime.UtcNow;
            this.questionnaireListViewItemStorage.Store(questionnaireListViewItem, questionnaireId);
        }

        private void Update(string questionnaireId, string title, bool isPublic)
        {
            var questionnaireListViewItem = this.questionnaireListViewItemStorage.GetById(questionnaireId);
            if (questionnaireListViewItem == null) return;

            questionnaireListViewItem.Title = title;
            questionnaireListViewItem.IsPublic = isPublic;
            questionnaireListViewItem.LastEntryDate = DateTime.UtcNow;

            this.questionnaireListViewItemStorage.Store(questionnaireListViewItem, questionnaireId);
        }

        private void Delete(string questionnaireId)
        {
            var questionnaireListViewItem = this.questionnaireListViewItemStorage.GetById(questionnaireId);
            if (questionnaireListViewItem == null) return;

            questionnaireListViewItem.IsDeleted = true;
            questionnaireListViewItem.LastEntryDate = DateTime.UtcNow;

            this.questionnaireListViewItemStorage.Store(questionnaireListViewItem, questionnaireId);
        }

        private void RemoveSharedPerson(string questionnaireId, Guid responsibleId, Guid personId, string personEmail)
        {
            var questionnaireListViewItem = this.questionnaireListViewItemStorage.GetById(questionnaireId);
            if (questionnaireListViewItem == null) return;

            if (questionnaireListViewItem.SharedPersons.Any(x => x.UserId == personId))
            {
                var toRemove = questionnaireListViewItem.SharedPersons.Where(x => x.UserId == personId).ToList();
                toRemove.ForEach(x => questionnaireListViewItem.SharedPersons.Remove(x));
            }

            questionnaireListViewItem.LastEntryDate = DateTime.UtcNow;
            this.questionnaireListViewItemStorage.Store(questionnaireListViewItem, questionnaireId);

            this.SendEmailNotifications(questionnaireListViewItem.Title, questionnaireListViewItem.CreatedBy, responsibleId,
                ShareChangeType.StopShare, personEmail, questionnaireListViewItem.QuestionnaireId, ShareType.Edit);
        }

        private void AddSharedPerson(string questionnaireId, Guid responsibleId, Guid personId, string personEmail, ShareType shareType)
        {
            var questionnaireListViewItem = this.questionnaireListViewItemStorage.GetById(questionnaireId);
            if (questionnaireListViewItem == null) return;

            if (questionnaireListViewItem.SharedPersons.Any(x => x.UserId == personId))
            {
                return;
            }

            var sharedPerson = new SharedPerson
            {
                UserId = personId,
                Email = personEmail,
                ShareType = shareType
            };

            questionnaireListViewItem.SharedPersons.Add(sharedPerson);

            questionnaireListViewItem.LastEntryDate = DateTime.UtcNow;
            this.questionnaireListViewItemStorage.Store(questionnaireListViewItem, questionnaireId);

            this.SendEmailNotifications(questionnaireListViewItem.Title, questionnaireListViewItem.CreatedBy, responsibleId,
                ShareChangeType.Share, personEmail, questionnaireListViewItem.QuestionnaireId, shareType);
        }

        private void SendEmailNotifications(string questionnaireTitle, Guid? questionnaireOwnerId, Guid responsibleId,
            ShareChangeType shareChangeType, string mailTo, string questionnaireId, ShareType shareType)
        {
            string mailFrom = this.accountStorage.GetById(responsibleId.FormatGuid())?.Email;
            string mailToUserName = this.accountRepository.GetUserNameByEmail(mailTo);

            this.emailNotifier.NotifyTargetPersonAboutShareChange(shareChangeType, mailTo, mailToUserName, questionnaireId,
                questionnaireTitle, shareType, mailFrom);

            if (!questionnaireOwnerId.HasValue || questionnaireOwnerId.Value == responsibleId) return;

            var questionnaireOwner = this.accountStorage.GetById(questionnaireOwnerId.Value.FormatGuid());
            if (questionnaireOwner != null)
            {
                this.emailNotifier.NotifyOwnerAboutShareChange(shareChangeType, questionnaireOwner.Email, questionnaireOwner.UserName,
                    questionnaireId, questionnaireTitle, shareType, mailFrom, mailTo);
            }
        }

        public void Process(Questionnaire aggregate, ImportQuestionnaire command)
            => this.Create(command.Source, true, command.QuestionnaireId);

        public void Process(Questionnaire aggregate, CloneQuestionnaire command)
            => this.Create(command.Source, false, command.QuestionnaireId, command.Title,
                command.ResponsibleId, command.IsPublic, DateTime.UtcNow);

        public void Process(Questionnaire aggregate, CreateQuestionnaire command) => this.Create(command);

        public void Process(Questionnaire aggregate, UpdateQuestionnaire command)
            => this.Update(command.QuestionnaireId.FormatGuid(), command.Title, command.IsPublic);

        public void Process(Questionnaire aggregate, DeleteQuestionnaire command) => this.Delete(command.QuestionnaireId.FormatGuid());

        public void Process(Questionnaire aggregate, AddSharedPersonToQuestionnaire command)
            => this.AddSharedPerson(command.QuestionnaireId.FormatGuid(), command.ResponsibleId, command.PersonId, command.EmailOrLogin, command.ShareType);

        public void Process(Questionnaire aggregate, RemoveSharedPersonFromQuestionnaire command)
            => this.RemoveSharedPerson(command.QuestionnaireId.FormatGuid(), command.ResponsibleId, command.PersonId, command.Email);

        public void Process(Questionnaire aggregate, AddStaticText command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, UpdateStaticText command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, MoveStaticText command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, DeleteStaticText command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, AddVariable command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, UpdateVariable command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, MoveVariable command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, DeleteVariable command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, AddMacro command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, UpdateMacro command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, DeleteMacro command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, AddOrUpdateAttachment command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, DeleteAttachment command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, AddOrUpdateTranslation command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, DeleteTranslation command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, SetDefaultTranslation command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, AddGroup command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, UpdateGroup command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, MoveGroup command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, DeleteGroup command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, PasteAfter command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, PasteInto command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, AddDefaultTypeQuestion command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, DeleteQuestion command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, MoveQuestion command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, UpdateMultimediaQuestion command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, UpdateDateTimeQuestion command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, UpdateNumericQuestion command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, UpdateQRBarcodeQuestion command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, UpdateGpsCoordinatesQuestion command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, UpdateTextListQuestion command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, UpdateTextQuestion command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, UpdateMultiOptionQuestion command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, UpdateSingleOptionQuestion command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, AddLookupTable command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, UpdateLookupTable command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, DeleteLookupTable command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, UpdateCascadingComboboxOptions command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, UpdateFilteredComboboxOptions command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, UpdateAreaQuestion command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, RevertVersionQuestionnaire command)
            => this.Update(command.QuestionnaireId.FormatGuid(), aggregate.QuestionnaireDocument.Title, aggregate.QuestionnaireDocument.IsPublic);

        public void Process(Questionnaire aggregate, UpdateAudioQuestion command) => this.Update(command.QuestionnaireId.FormatGuid());

        public void Process(Questionnaire aggregate, UpdateMetadata command)
            => this.Update(command.QuestionnaireId.FormatGuid(), command.Title, aggregate.QuestionnaireDocument.IsPublic);
    }
}