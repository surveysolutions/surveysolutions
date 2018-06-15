using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.BoundedContexts.Designer.QuestionnaireCompilationForOldVersions;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo
{
    public class QuestionnaireInfoViewFactory : IQuestionnaireInfoViewFactory
    {
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader;
        private readonly IPlainStorageAccessor<QuestionnaireListViewItem> questionnaires;
        private readonly IQuestionnaireCompilationVersionService questionnaireCompilationVersion;
        private readonly IPlainStorageAccessor<Aggregates.User> accountsStorage;
        private readonly IAttachmentService attachmentService;
        private readonly IMembershipUserService membershipUserService;

        public QuestionnaireInfoViewFactory(
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader,
            IPlainStorageAccessor<QuestionnaireListViewItem> questionnaires,
            IQuestionnaireCompilationVersionService questionnaireCompilationVersion,
            IPlainStorageAccessor<Aggregates.User> accountsStorage,
            IAttachmentService attachmentService,
            IMembershipUserService membershipUserService)
        {
            this.questionnaireDocumentReader = questionnaireDocumentReader;
            this.questionnaires = questionnaires;
            this.questionnaireCompilationVersion = questionnaireCompilationVersion;
            this.accountsStorage = accountsStorage;
            this.attachmentService = attachmentService;
            this.membershipUserService = membershipUserService;
        }

        public QuestionnaireInfoView Load(string questionnaireId, Guid viewerId)
        {
            QuestionnaireDocument questionnaireDocument = this.questionnaireDocumentReader.GetById(questionnaireId);

            var questionnaireInfoView = new QuestionnaireInfoView
            {
                QuestionnaireId = questionnaireId,
                Title = questionnaireDocument.Title,
                Variable = questionnaireDocument.VariableName,
                Chapters = new List<ChapterInfoView>(),
                IsPublic = questionnaireDocument.IsPublic
            };

            foreach (IGroup chapter in questionnaireDocument.Children.OfType<IGroup>())
            {
                questionnaireInfoView.Chapters.Add(new ChapterInfoView
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

                if (item is IGroup group)
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

            var listItem = this.questionnaires.GetById(questionnaireId);
            var sharedPersons = listItem.SharedPersons.GroupBy(x => x.Email).Select(g => g.First())
                .Select(x => new SharedPersonView
                {
                    Email = x.Email,
                    Login = this.accountsStorage.GetById(x.UserId.FormatGuid()).UserName,
                    UserId = x.UserId,
                    IsOwner = x.IsOwner,
                    ShareType = x.ShareType
                })
                .ToList();
            
            if (questionnaireDocument.CreatedBy.HasValue &&
                sharedPersons.All(x => x.UserId != questionnaireDocument.CreatedBy))
            {
                var owner = this.accountsStorage.GetById(questionnaireDocument.CreatedBy.Value.FormatGuid());
                if (owner != null)
                {
                    sharedPersons.Add(new SharedPersonView
                    {
                        Email = owner.Email,
                        Login = owner.UserName,
                        UserId = questionnaireDocument.CreatedBy.Value,
                        IsOwner = true
                    });
                }
            }

            var person = sharedPersons.FirstOrDefault(sharedPerson => sharedPerson.UserId == viewerId);

            questionnaireInfoView.SharedPersons = sharedPersons;
            questionnaireInfoView.IsReadOnlyForUser = person == null || (!person.IsOwner && person.ShareType != ShareType.Edit);
            questionnaireInfoView.HasViewerAdminRights = this.membershipUserService.WebUser.IsAdmin;
            questionnaireInfoView.WebTestAvailable = this.questionnaireCompilationVersion.GetById(listItem.PublicId)?.Version == null;

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

            questionnaireInfoView.Attachments = questionnaireDocument.Attachments
                .Select(attachmentIdentity => new AttachmentView
                {
                    AttachmentId = attachmentIdentity.AttachmentId.FormatGuid(),
                    Name = attachmentIdentity.Name,
                    Content = this.attachmentService.GetContentDetails(attachmentIdentity.ContentId),
                    Meta = attachments?.FirstOrDefault(x => x.AttachmentId == attachmentIdentity.AttachmentId)
                })
                .OrderBy(x => x.Name)
                .ToList();


            questionnaireInfoView.Translations = questionnaireDocument.Translations
                .Select(translationIdentity => new TranslationView
                {
                    TranslationId = translationIdentity.Id.FormatGuid(),
                    Name = translationIdentity.Name,
                    IsDefault = translationIdentity.Id == questionnaireDocument.DefaultTranslation
                })
                .OrderBy(x => x.Name)
                .ToList();

            questionnaireInfoView.Metadata = new MetadataView()
            {
                Title = questionnaireDocument.Title,
                SubTitle = questionnaireDocument.Metadata?.SubTitle,
                Version = questionnaireDocument.Metadata?.Version,
                KindOfData = questionnaireDocument.Metadata?.KindOfData,
                VersionNotes = questionnaireDocument.Metadata?.VersionNotes,
                PrimaryInvestigator = questionnaireDocument.Metadata?.PrimaryInvestigator,
                Year = questionnaireDocument.Metadata?.Year,
                Language = questionnaireDocument.Metadata?.Language,
                Country = questionnaireDocument.Metadata?.Country,
                ModeOfDataCollection = questionnaireDocument.Metadata?.ModeOfDataCollection,
                UnitOfAnalysis = questionnaireDocument.Metadata?.UnitOfAnalysis,
                AgreeToMakeThisQuestionnairePublic = questionnaireDocument.Metadata?.AgreeToMakeThisQuestionnairePublic ?? false,
                Universe = questionnaireDocument.Metadata?.Universe,
                Funding = questionnaireDocument.Metadata?.Funding,
                Coverage = questionnaireDocument.Metadata?.Coverage,
                Notes = questionnaireDocument.Metadata?.Notes,
                Consultant = questionnaireDocument.Metadata?.Consultant,
                StudyType = questionnaireDocument.Metadata?.StudyType,
                Keywords = questionnaireDocument.Metadata?.Keywords,
            };

            questionnaireInfoView.StudyTypes = StudyTypeProvider.GetStudyTypeItems();
            questionnaireInfoView.KindsOfData = KindOfDataProvider.GetKindOfDataItems();
            questionnaireInfoView.Countries = CountryListProvider.GetCounryItems();
            questionnaireInfoView.ModesOfDataCollection = ModeOfDataCollectionProvider.GetModeOfDataCollectionItems();

            return questionnaireInfoView;
        }
    }
}
