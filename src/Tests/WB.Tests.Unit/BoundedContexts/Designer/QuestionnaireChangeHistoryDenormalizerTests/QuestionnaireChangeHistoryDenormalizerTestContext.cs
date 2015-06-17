using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireChangeHistoryDenormalizerTests
{
    [Subject(typeof(QuestionnaireChangeHistoryDenormalizer))]
    internal class QuestionnaireChangeHistoryDenormalizerTestContext
    {
        protected static QuestionnaireChangeHistoryDenormalizer CreateQuestionnaireChangeHistoryDenormalizer(
            IReadSideRepositoryWriter<AccountDocument> accountDocumentStorage=null,
            IReadSideRepositoryWriter<QuestionnaireChangeRecord> questionnaireChangeRecord = null,
            IReadSideKeyValueStorage<QuestionnaireStateTacker> questionnaireStateTacker = null
            )
        {
            return new QuestionnaireChangeHistoryDenormalizer(accountDocumentStorage??Mock.Of<IReadSideRepositoryWriter<AccountDocument>>(),
                questionnaireChangeRecord ?? Mock.Of<IReadSideRepositoryWriter<QuestionnaireChangeRecord>>(),
                questionnaireStateTacker??Mock.Of<IReadSideKeyValueStorage<QuestionnaireStateTacker>>());
        }

        protected static QuestionnaireChangeRecord GetFirstChangeRecord(IQueryableReadSideRepositoryReader<QuestionnaireChangeRecord> questionnaireChangeRecord, string questionnaireId)
        {
            return
                questionnaireChangeRecord.Query(_ => _.FirstOrDefault(r => r.QuestionnaireId == questionnaireId));
        }

        protected static QuestionnaireChangeRecord[] GetAllRecords(IQueryableReadSideRepositoryReader<QuestionnaireChangeRecord> questionnaireChangeRecord)
        {
            return
                questionnaireChangeRecord.Query(_ => _.ToArray());
        }

        protected static QuestionnaireChangeRecord GetFirstChangeRecord(IQueryableReadSideRepositoryReader<QuestionnaireChangeRecord> questionnaireChangeRecord, Guid questionnaireId)
        {
            return GetFirstChangeRecord(questionnaireChangeRecord, questionnaireId.FormatGuid());
        }
    }
}
