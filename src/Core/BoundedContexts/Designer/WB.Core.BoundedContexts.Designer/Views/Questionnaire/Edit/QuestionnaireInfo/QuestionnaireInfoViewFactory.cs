using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Shared.Web.Membership;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo
{
    public class QuestionnaireInfoViewFactory : IQuestionnaireInfoViewFactory
    {
        private readonly IPlainKeyValueStorage<QuestionnaireSharedPersons> sharedPersonsStorage;
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader;
        private readonly IPlainStorageAccessor<Aggregates.Account> accountsDocumentReader;
        private readonly IAttachmentService attachmentService;
        private readonly IMembershipUserService membershipUserService;

        public QuestionnaireInfoViewFactory(
            IPlainKeyValueStorage<QuestionnaireSharedPersons> sharedPersonsStorage,
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader,
            IPlainStorageAccessor<Aggregates.Account> accountsDocumentReader,
            IAttachmentService attachmentService,
            IMembershipUserService membershipUserService)
        {
            this.sharedPersonsStorage = sharedPersonsStorage;
            this.questionnaireDocumentReader = questionnaireDocumentReader;
            this.accountsDocumentReader = accountsDocumentReader;
            this.attachmentService = attachmentService;
            this.membershipUserService = membershipUserService;
        }

        public QuestionnaireInfoView Load(string questionnaireId, Guid viewerId)
        {
            QuestionnaireDocument questionnaireDocument = this.questionnaireDocumentReader.GetById(questionnaireId);

            var questionnaireInfoView = new QuestionnaireInfoView()
            {
                QuestionnaireId = questionnaireId,
                Title = questionnaireDocument.Title,
                Chapters = new List<ChapterInfoView>(),
                IsPublic = questionnaireDocument.IsPublic
            };

            foreach (IGroup chapter in questionnaireDocument.Children.OfType<IGroup>())
            {
                questionnaireInfoView.Chapters.Add(new ChapterInfoView()
                {
                    ItemId = chapter.PublicKey.FormatGuid(),
                    Title = chapter.Title,
                    GroupsCount = 0,
                    RostersCount = 0,
                    QuestionsCount = 0
                });
            }

            int questionsCount = 0, groupsCount = 0, rostersCount = 0;
            questionnaireDocument.Children.TreeToEnumerable(item => item.Children).ForEach(item =>
            {
                if (item is IQuestion)
                {
                    questionsCount++;
                    return;
                }
                var group = item as IGroup;
                if (group != null)
                {
                    if (group.IsRoster)
                    {
                        rostersCount++;
                    }
                    else
                    {
                        groupsCount++;
                    }
                }
            });
            questionnaireInfoView.QuestionsCount = questionsCount;
            questionnaireInfoView.GroupsCount = groupsCount;
            questionnaireInfoView.RostersCount = rostersCount;

            var sharedPersons = this.sharedPersonsStorage.GetById(questionnaireId)?.SharedPersons ?? new List<SharedPerson>();
            
            if (questionnaireDocument.CreatedBy.HasValue &&
                sharedPersons.All(x => x.Id != questionnaireDocument.CreatedBy))
            {
                var owner = accountsDocumentReader.GetById(questionnaireDocument.CreatedBy.Value);
                if (owner != null)
                {
                    sharedPersons.Insert(0, new SharedPerson
                    {
                        Email = owner.Email,
                        Id = questionnaireDocument.CreatedBy.Value,
                        IsOwner = true
                    });
                }
            }

            var person = sharedPersons.FirstOrDefault(sharedPerson => sharedPerson.Id == viewerId);

            questionnaireInfoView.SharedPersons = sharedPersons;
            questionnaireInfoView.IsReadOnlyForUser = person == null || (!person.IsOwner && person.ShareType != ShareType.Edit);
            questionnaireInfoView.HasViewerAdminRights = this.membershipUserService.WebUser.IsAdmin;

            questionnaireInfoView.Macros = questionnaireDocument
                .Macros
                .Select(x => new MacroView { ItemId = x.Key.FormatGuid(), Name = x.Value.Name, Description = x.Value.Description, Content = x.Value.Content })
                .OrderBy(x => x.Name)
                .ToList();

            questionnaireInfoView.LookupTables = questionnaireDocument
                .LookupTables
                .Select(x => new LookupTableView
                        {
                            ItemId = x.Key.FormatGuid(),
                            Name = x.Value.TableName ?? "",
                            FileName = x.Value.FileName ?? ""
                        })
                .OrderBy(x => x.Name)
                .ToList();

            
            var attachments = this.attachmentService.GetAttachmentsByQuestionnaire(questionnaireDocument.PublicKey);
            attachments?.ForEach(x => x.LastUpdateDate = x.LastUpdateDate.ToLocalTime());

            questionnaireInfoView.Attachments = questionnaireDocument.Attachments
                .Select(attachmentIdentity => new AttachmentView
                {
                    AttachmentId = attachmentIdentity.AttachmentId.FormatGuid(),
                    Name = attachmentIdentity.Name,
                    Content = this.attachmentService.GetContentDetails(attachmentIdentity.ContentId),
                    Meta = attachments.FirstOrDefault(x => x.AttachmentId == attachmentIdentity.AttachmentId)
                })
                .OrderBy(x => x.Name)
                .ToList();


            questionnaireInfoView.Translations = questionnaireDocument.Translations
                .Select(translationIdentity => new TranslationView
                {
                    TranslationId = translationIdentity.Id.FormatGuid(),
                    Name = translationIdentity.Name
                })
                .OrderBy(x => x.Name)
                .ToList();

            return questionnaireInfoView;
        }
    }
}