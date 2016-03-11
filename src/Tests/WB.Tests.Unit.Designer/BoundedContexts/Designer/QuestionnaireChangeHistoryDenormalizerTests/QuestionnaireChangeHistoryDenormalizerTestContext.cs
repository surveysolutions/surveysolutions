﻿using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireChangeHistoryDenormalizerTests
{
    [Subject(typeof(QuestionnaireChangeHistoryDenormalizer))]
    internal class QuestionnaireChangeHistoryDenormalizerTestContext
    {
        protected static QuestionnaireChangeHistoryDenormalizer CreateQuestionnaireChangeHistoryDenormalizer(
            IReadSideRepositoryWriter<AccountDocument> accountDocumentStorage=null,
            IReadSideRepositoryWriter<QuestionnaireChangeRecord> questionnaireChangeRecord = null,
            IReadSideKeyValueStorage<QuestionnaireStateTracker> questionnaireStateTacker = null
            )
        {
            return new QuestionnaireChangeHistoryDenormalizer(accountDocumentStorage??Mock.Of<IReadSideRepositoryWriter<AccountDocument>>(),
                questionnaireChangeRecord ?? Mock.Of<IReadSideRepositoryWriter<QuestionnaireChangeRecord>>(),
                questionnaireStateTacker??Mock.Of<IReadSideKeyValueStorage<QuestionnaireStateTracker>>());
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
