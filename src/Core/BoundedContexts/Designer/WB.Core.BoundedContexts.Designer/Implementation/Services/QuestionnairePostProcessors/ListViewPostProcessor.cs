using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Microsoft.EntityFrameworkCore;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Categories;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Variable;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;

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
        ICommandPostProcessor<Questionnaire, ReplaceTextsCommand>,
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
        ICommandPostProcessor<Questionnaire, ReplaceOptionsWithClassification>,
        ICommandPostProcessor<Questionnaire, UpdateFilteredComboboxOptions>,
        ICommandPostProcessor<Questionnaire, RevertVersionQuestionnaire>,
        ICommandPostProcessor<Questionnaire, UpdateAreaQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateAudioQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateMetadata>,
        ICommandPostProcessor<Questionnaire, PassOwnershipFromQuestionnaire>,
        ICommandPostProcessor<Questionnaire, AddOrUpdateCategories>,
        ICommandPostProcessor<Questionnaire, DeleteCategories>
    {
        private readonly DesignerDbContext dbContext;
        private readonly IRecipientNotifier emailNotifier;

        public ListViewPostProcessor(DesignerDbContext dbContext, IRecipientNotifier emailNotifier)
        {
            this.dbContext = dbContext;
            this.emailNotifier = emailNotifier;
        }

        private void Create(QuestionnaireDocument document, bool shouldPreserveSharedPersons, 
            Guid? questionnaireId = null, string? questionnaireTitle = null, Guid? createdBy = null,
            bool? isPublic = null, DateTime? creationDate = null)
        {
            var questionnaireIdValue = questionnaireId ?? document.PublicKey;

            Guid? creatorId = createdBy ?? document.CreatedBy;

            var sourceQuestionnaireListViewItem =
                this.dbContext.Questionnaires.Include(x => x.SharedPersons)
                    .FirstOrDefault(x => x.QuestionnaireId == questionnaireIdValue.FormatGuid());
            
            
            var questionnaireListViewItem = sourceQuestionnaireListViewItem ?? new QuestionnaireListViewItem();
            questionnaireListViewItem.PublicId = questionnaireIdValue;
            questionnaireListViewItem.Title = questionnaireTitle ?? document.Title;
            questionnaireListViewItem.CreationDate = creationDate ?? document.CreationDate;
            questionnaireListViewItem.LastEntryDate = DateTime.UtcNow;
            questionnaireListViewItem.CreatedBy = creatorId.GetValueOrDefault();
            questionnaireListViewItem.IsPublic = isPublic ?? document.IsPublic;
            questionnaireListViewItem.Owner = null;
            questionnaireListViewItem.CreatorName = string.Empty;
            questionnaireListViewItem.IsDeleted = false;

            if (creatorId.HasValue)
                questionnaireListViewItem.CreatorName = this.dbContext.Users.Find(creatorId.Value)?.UserName ?? String.Empty;

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

            bool existing =
                this.dbContext.Questionnaires.Any(x => x.QuestionnaireId == questionnaireListViewItem.QuestionnaireId);
            if (existing)
            {
                this.dbContext.Questionnaires.Update(questionnaireListViewItem);
            }
            else
            {
                this.dbContext.Questionnaires.Add(questionnaireListViewItem);
            }
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
                CreatorName = this.dbContext.Users.Find(command.ResponsibleId)?.UserName ?? String.Empty
            };
            this.dbContext.Questionnaires.Add(questionnaireListViewItem);
        }

        private void Update(string questionnaireId)
        {
            var questionnaireListViewItem = this.dbContext.Questionnaires.Find(questionnaireId);
            if (questionnaireListViewItem == null) return;

            questionnaireListViewItem.LastEntryDate = DateTime.UtcNow;
        }

        private void Update(string questionnaireId, string title, bool isPublic)
        {
            var questionnaireListViewItem = this.dbContext.Questionnaires.Find(questionnaireId);
            if (questionnaireListViewItem == null) return;

            questionnaireListViewItem.Title = title;
            questionnaireListViewItem.IsPublic = isPublic;
            questionnaireListViewItem.LastEntryDate = DateTime.UtcNow;
        }

        private void Delete(string questionnaireId)
        {
            var questionnaireListViewItem = this.dbContext.Questionnaires.Find(questionnaireId);
            if (questionnaireListViewItem == null) return;

            questionnaireListViewItem.IsDeleted = true;
            questionnaireListViewItem.LastEntryDate = DateTime.UtcNow;
        }

        private void RemoveSharedPerson(string questionnaireId, Guid responsibleId, Guid personId, string personEmail)
        {
            var questionnaireListViewItem = this.dbContext.Questionnaires.Find(questionnaireId);
            if (questionnaireListViewItem == null) return;

            if (questionnaireListViewItem.SharedPersons.Any(x => x.UserId == personId))
            {
                var toRemove = questionnaireListViewItem.SharedPersons.Where(x => x.UserId == personId).ToList();
                toRemove.ForEach(x => questionnaireListViewItem.SharedPersons.Remove(x));
            }

            questionnaireListViewItem.LastEntryDate = DateTime.UtcNow;
            this.dbContext.Questionnaires.Update(questionnaireListViewItem);

            var questionnaireOwnerId = questionnaireListViewItem.CreatedBy;
            this.SendEmailNotifications(questionnaireListViewItem.Title, questionnaireOwnerId, responsibleId,
                ShareChangeType.StopShare, personEmail, questionnaireListViewItem.QuestionnaireId, ShareType.Edit);
        }
        
        private void AddSharedPerson(string questionnaireId, Guid responsibleId, Guid personId, string personEmail, ShareType shareType)
        {
            var questionnaireListViewItem = this.dbContext.Questionnaires.Find(questionnaireId);
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
            this.dbContext.Questionnaires.Update(questionnaireListViewItem);

            this.SendEmailNotifications(questionnaireListViewItem.Title, questionnaireListViewItem.CreatedBy, responsibleId,
                ShareChangeType.Share, personEmail, questionnaireListViewItem.QuestionnaireId, shareType);
        }
               
        public void Process(Questionnaire aggregate, PassOwnershipFromQuestionnaire command)
        {
            var questionnaireListViewItem = this.dbContext.Questionnaires.Find(command.QuestionnaireId.FormatGuid());
            if (questionnaireListViewItem == null) return;

            var newOwner = this.dbContext.Users.SingleOrDefault(u => u.Id == command.NewOwnerId);
            if (newOwner == null) return;
            
            questionnaireListViewItem.Owner = newOwner.UserName;
            questionnaireListViewItem.CreatedBy = newOwner.Id;

            if (questionnaireListViewItem.SharedPersons.Any(x => x.UserId == newOwner.Id))
            {
                var toRemove = questionnaireListViewItem.SharedPersons.Where(x => x.UserId == newOwner.Id).ToList();
                toRemove.ForEach(x => questionnaireListViewItem.SharedPersons.Remove(x));
            }

            var sharedPerson = new SharedPerson
            {
                UserId = command.ResponsibleId,
                Email = command.OwnerEmail,
                ShareType = ShareType.Edit
            };

            questionnaireListViewItem.SharedPersons.Add(sharedPerson);

            questionnaireListViewItem.LastEntryDate = DateTime.UtcNow;
            this.dbContext.Questionnaires.Update(questionnaireListViewItem);

            // notify old owner that he is now has Edit permissions
            this.SendEmailNotifications(questionnaireListViewItem.Title, questionnaireListViewItem.CreatedBy, newOwner.Id,
               ShareChangeType.Share, command.OwnerEmail, questionnaireListViewItem.QuestionnaireId, ShareType.Edit);

            // notify new owner that he is now has owner of questionnaire
            this.SendEmailNotifications(questionnaireListViewItem.Title, questionnaireListViewItem.CreatedBy, command.ResponsibleId,
               ShareChangeType.TransferOwnership, command.NewOwnerEmail, questionnaireListViewItem.QuestionnaireId, ShareType.Edit);
        }

        private void SendEmailNotifications(string questionnaireTitle,
            Guid? questionnaireOwnerId,
            Guid responsibleId,
            ShareChangeType shareChangeType, 
            string mailTo, 
            string questionnaireId, 
            ShareType shareType)
        {
            string? mailFrom = this.dbContext.Users.Find(responsibleId)?.Email;
            string? mailToUserName = this.dbContext.Users.FirstOrDefault(x => x.NormalizedEmail == mailTo.ToUpper())?.UserName;

            this.emailNotifier.NotifyTargetPersonAboutShareChange(
                shareChangeType, 
                mailTo, 
                mailToUserName, 
                questionnaireId,
                questionnaireTitle, shareType,
                mailFrom);

            if (!questionnaireOwnerId.HasValue || questionnaireOwnerId.Value == responsibleId) return;

            var questionnaireOwner = this.dbContext.Users.Find(questionnaireOwnerId.Value);
            if (questionnaireOwner != null && shareChangeType != ShareChangeType.TransferOwnership)
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
        public void Process(Questionnaire aggregate, ReplaceTextsCommand command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, AddLookupTable command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, UpdateLookupTable command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, DeleteLookupTable command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, UpdateCascadingComboboxOptions command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, ReplaceOptionsWithClassification command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, UpdateFilteredComboboxOptions command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, UpdateAreaQuestion command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, RevertVersionQuestionnaire command)
            => this.Update(command.QuestionnaireId.FormatGuid(), aggregate.QuestionnaireDocument.Title, aggregate.QuestionnaireDocument.IsPublic);

        public void Process(Questionnaire aggregate, UpdateAudioQuestion command) => this.Update(command.QuestionnaireId.FormatGuid());

        public void Process(Questionnaire aggregate, UpdateMetadata command)
            => this.Update(command.QuestionnaireId.FormatGuid(), command.Title, aggregate.QuestionnaireDocument.IsPublic);
        public void Process(Questionnaire aggregate, AddOrUpdateCategories command) => this.Update(command.QuestionnaireId.FormatGuid());
        public void Process(Questionnaire aggregate, DeleteCategories command) => this.Update(command.QuestionnaireId.FormatGuid());
    }
}
