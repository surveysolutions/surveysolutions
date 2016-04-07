using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf
{
    public interface IPdfFactory
    {
        PdfQuestionnaireModel Load(Guid questionnaireId, Guid requestedByUserId, string requestedByUserName);
        string LoadQuestionnaireTitle(Guid questionnaireId);
    }

    public class PdfFactory : IPdfFactory
    {
        private readonly IQueryableReadSideRepositoryReader<QuestionnaireChangeRecord> questionnaireChangeHistoryStorage;
        private readonly IQueryableReadSideRepositoryReader<QuestionnaireListViewItem> questionnaireListViewItemStorage;
        private readonly IReadSideKeyValueStorage<QuestionnaireDocument> questionnaireStorage;
        private readonly IReadSideRepositoryReader<AccountDocument> accountsDocumentReader;
        private readonly IReadSideKeyValueStorage<QuestionnaireSharedPersons> sharedPersonsStorage;

        public PdfFactory(
            IReadSideKeyValueStorage<QuestionnaireDocument> questionnaireStorage,
            IQueryableReadSideRepositoryReader<QuestionnaireChangeRecord> questionnaireChangeHistoryStorage, 
            IReadSideRepositoryReader<AccountDocument> accountsDocumentReader, 
            IQueryableReadSideRepositoryReader<QuestionnaireListViewItem> questionnaireListViewItemStorage, 
            IReadSideKeyValueStorage<QuestionnaireSharedPersons> sharedPersonsStorage)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.questionnaireChangeHistoryStorage = questionnaireChangeHistoryStorage;
            this.accountsDocumentReader = accountsDocumentReader;
            this.questionnaireListViewItemStorage = questionnaireListViewItemStorage;
            this.sharedPersonsStorage = sharedPersonsStorage;
        }

        public PdfQuestionnaireModel Load(Guid questionnaireId, Guid requestedByUserId, string requestedByUserName)
        {
            var questionnaire = this.questionnaireStorage.GetById(questionnaireId);
            if (questionnaire == null || questionnaire.IsDeleted)
            {
                return null;
            }

            var sharedPersons = sharedPersonsStorage.GetById(questionnaireId)?.SharedPersons ?? new List<SharedPerson>();

            var modificationStatisticsByUsers = questionnaireChangeHistoryStorage.Query(_ => _
                .Where(x => x.QuestionnaireId == questionnaireId.FormatGuid())
                .GroupBy(x => new { x.UserId, x.UserName })
                .Select(grouping => new PdfQuestionnaireModel.ModificationStatisticsByUser
                {
                    UserId = grouping.Key.UserId,
                    Date = grouping.Max(x => x.Timestamp),
                    Name = grouping.Key.UserName,
                })).ToList();

            var pdfView = new PdfQuestionnaireModel(questionnaire, new PdfSettings())
            {
                Requested = new PdfQuestionnaireModel.ModificationStatisticsByUser
                {
                    UserId = requestedByUserId,
                    Name = requestedByUserName,
                    Date = DateTime.Now
                },
                Created = new PdfQuestionnaireModel.ModificationStatisticsByUser
                {
                    UserId = questionnaire.CreatedBy ?? Guid.Empty,
                    Name = questionnaire.CreatedBy.HasValue
                        ? accountsDocumentReader.GetById(questionnaire.CreatedBy.Value)?.UserName
                        : string.Empty,
                    Date = questionnaire.CreationDate
                },
                LastModified = modificationStatisticsByUsers.OrderByDescending(x => x.Date).First(),
                SharedPersons = sharedPersons.Select(person => new PdfQuestionnaireModel.ModificationStatisticsByUser
                {
                    UserId = person.Id,
                    Name = accountsDocumentReader.GetById(person.Id)?.UserName,
                    Date = modificationStatisticsByUsers.FirstOrDefault(x => x.UserId == person.Id)?.Date
                })
            };
            return pdfView;
        }

        public string LoadQuestionnaireTitle(Guid questionnaireId)
        {
            return this.questionnaireListViewItemStorage.GetById(questionnaireId.FormatGuid()).Title;
        }
    }
}
