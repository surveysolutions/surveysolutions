using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo
{
    public class QuestionnaireInfoViewFactory : IQuestionnaireInfoViewFactory
    {
        private readonly IReadSideKeyValueStorage<QuestionnaireInfoView> questionnaireStorage;
        private readonly IReadSideKeyValueStorage<QuestionnaireSharedPersons> sharedPersons;
        private readonly IReadSideKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader;
        private readonly IReadSideRepositoryReader<AccountDocument> accountsDocumentReader;
        private readonly IAttachmentService attachmentService;

        public QuestionnaireInfoViewFactory(IReadSideKeyValueStorage<QuestionnaireInfoView> questionnaireStorage,
            IReadSideKeyValueStorage<QuestionnaireSharedPersons> sharedPersons,
            IReadSideKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader,
            IReadSideRepositoryReader<AccountDocument> accountsDocumentReader,
            IAttachmentService attachmentService)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.sharedPersons = sharedPersons;
            this.questionnaireDocumentReader = questionnaireDocumentReader;
            this.accountsDocumentReader = accountsDocumentReader;
            this.attachmentService = attachmentService;
        }

        public QuestionnaireInfoView Load(string questionnaireId, Guid personId)
        {
            QuestionnaireInfoView questionnaireInfoView = this.questionnaireStorage.GetById(questionnaireId);

            if (questionnaireInfoView == null)
                return null;

            QuestionnaireDocument questionnaireDocument = this.questionnaireDocumentReader.GetById(questionnaireId);
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

            var sharedPersonsList = new List<SharedPerson>();

            QuestionnaireSharedPersons questionnaireSharedPersons = sharedPersons.GetById(questionnaireId);
            if (questionnaireSharedPersons != null)
            {
                sharedPersonsList = questionnaireSharedPersons.SharedPersons;
            }

            if (questionnaireDocument.CreatedBy.HasValue)
            {
                var owner = accountsDocumentReader.GetById(questionnaireDocument.CreatedBy.Value);
                if (owner != null)
                {
                    sharedPersonsList.Insert(0,
                        new SharedPerson() { Email = owner.Email, Id = questionnaireDocument.CreatedBy.Value, IsOwner = true });
                }
            }

            var person = sharedPersonsList.FirstOrDefault(x => x.Id == personId);

            if (person != null)
            {
                questionnaireInfoView.SharedPersons = sharedPersonsList;
                questionnaireInfoView.IsReadOnlyForUser = !person.IsOwner && person.ShareType != 0;
            }
            else
            {
                questionnaireInfoView.IsReadOnlyForUser = true;
            }

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

            var dbAttachments = this.attachmentService.GetAttachmentsForQuestionnaire(questionnaireId).ToList();

            questionnaireInfoView.Attachments = (from qAttachment in questionnaireDocument.Attachments
                join dbAttachment in dbAttachments on qAttachment.Key.FormatGuid() equals dbAttachment.ItemId into groupJoin
                from subAttachment in groupJoin.DefaultIfEmpty()
                select new AttachmentView
                {
                    ItemId = qAttachment.Key.FormatGuid(),
                    Type = subAttachment?.Type,
                    Name = qAttachment.Value.Name,
                    FileName = qAttachment.Value.FileName,
                    SizeInBytes = subAttachment?.SizeInBytes,
                    LastUpdated = subAttachment?.LastUpdated,
                    Meta = subAttachment?.Meta
                })
                .OrderBy(x => x.Name)
                .ToList();

            return questionnaireInfoView;
        }
    }
}