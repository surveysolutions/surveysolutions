﻿using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Microsoft.EntityFrameworkCore;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.QuestionnaireCompilationForOldVersions;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo
{
    public class QuestionnaireInfoViewFactory : IQuestionnaireInfoViewFactory
    {
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader;
        private readonly DesignerDbContext dbContext;
        private readonly IQuestionnaireCompilationVersionService questionnaireCompilationVersion;
        private readonly IAttachmentService attachmentService;
        private readonly ILoggedInUser loggedInUser;

        public QuestionnaireInfoViewFactory(
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader,
            DesignerDbContext dbContext,
            IQuestionnaireCompilationVersionService questionnaireCompilationVersion,
            IAttachmentService attachmentService,
            ILoggedInUser loggedInUser)
        {
            this.questionnaireDocumentReader = questionnaireDocumentReader;
            this.dbContext = dbContext;
            this.questionnaireCompilationVersion = questionnaireCompilationVersion;
            this.attachmentService = attachmentService;
            this.loggedInUser = loggedInUser;
        }

        public QuestionnaireInfoView Load(string questionnaireId, Guid viewerId)
        {
            QuestionnaireDocument questionnaireDocument = this.questionnaireDocumentReader.GetById(questionnaireId);

            if (questionnaireDocument == null) return null;

            var questionnaireInfoView = new QuestionnaireInfoView
            {
                QuestionnaireId = questionnaireId,
                Title = questionnaireDocument.Title,
                Variable = questionnaireDocument.VariableName,
                Chapters = new List<ChapterInfoView>(),
                IsPublic = questionnaireDocument.IsPublic,
                HideIfDisabled = questionnaireDocument.HideIfDisabled
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

            var listItem = this.dbContext.Questionnaires.Include(x => x.SharedPersons).FirstOrDefault(x => x.QuestionnaireId == questionnaireId);
            var sharedPersons = listItem.SharedPersons.GroupBy(x => x.Email).Select(g => g.First())
                .Select(x => new SharedPersonView
                {
                    Email = x.Email,
                    Login = this.dbContext.Users.Find(x.UserId).UserName,
                    UserId = x.UserId,
                    IsOwner = x.IsOwner,
                    ShareType = x.ShareType
                })
                .ToList();
            
            if (questionnaireDocument.CreatedBy.HasValue &&
                sharedPersons.All(x => x.UserId != questionnaireDocument.CreatedBy))
            {
                var owner = this.dbContext.Users.Find(questionnaireDocument.CreatedBy.Value);
                if (owner != null)
                {
                    sharedPersons.Insert(0, new SharedPersonView
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
            questionnaireInfoView.IsSharedWithUser = person != null;
            questionnaireInfoView.WebTestAvailable = this.questionnaireCompilationVersion.GetById(listItem.PublicId)?.Version == null;
            questionnaireInfoView.HasViewerAdminRights = this.loggedInUser.IsAdmin;

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

            questionnaireInfoView.Scenarios = dbContext.Scenarios
                .Where(s => s.QuestionnaireId == questionnaireDocument.PublicKey)
                .OrderBy(s => s.Id)
                .Select(s => new ScenarioView()
                {
                    Id = s.Id,
                    Title = s.Title
                })
                .ToList();

            return questionnaireInfoView;
        }
    }
}
