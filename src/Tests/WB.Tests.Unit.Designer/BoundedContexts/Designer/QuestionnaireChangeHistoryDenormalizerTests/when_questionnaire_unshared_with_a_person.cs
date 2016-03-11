﻿using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireChangeHistoryDenormalizerTests
{
    internal class when_questionnaire_unshared_with_a_person : QuestionnaireChangeHistoryDenormalizerTestContext
    {
        Establish context = () =>
        {
            questionnaireStateTackerStorage = new TestInMemoryWriter<QuestionnaireStateTracker>();
            questionnaireChangeRecordStorage = new TestInMemoryWriter<QuestionnaireChangeRecord>();
            questionnaireChangeHistoryDenormalizer =
                CreateQuestionnaireChangeHistoryDenormalizer(questionnaireStateTacker: questionnaireStateTackerStorage,
                    questionnaireChangeRecord: questionnaireChangeRecordStorage,
                    accountDocumentStorage:
                        Mock.Of<IReadSideRepositoryWriter<AccountDocument>>(
                        _ => _.GetById(Moq.It.IsAny<string>()) == Create.AccountDocument(userName)));
        };

        Because of = () =>
            questionnaireChangeHistoryDenormalizer.Handle(Create.SharedPersonFromQuestionnaireRemoved(questionnaireId: questionnaireId, personId: personId));

        It should_store_change_record_with_target_action_equal_to_delete = () =>
            GetFirstChangeRecord(questionnaireChangeRecordStorage, questionnaireId).ActionType.ShouldEqual(QuestionnaireActionType.Delete);

        It should_store_change_record_with_target_type_equal_to_person = () =>
            GetFirstChangeRecord(questionnaireChangeRecordStorage, questionnaireId).TargetItemType.ShouldEqual(QuestionnaireItemType.Person);

        It should_store_change_record_with_target_title_equal_to_test = () =>
            GetFirstChangeRecord(questionnaireChangeRecordStorage, questionnaireId).TargetItemTitle.ShouldEqual(userName);

        private static QuestionnaireChangeHistoryDenormalizer questionnaireChangeHistoryDenormalizer;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static Guid personId = Guid.Parse("22222222222222222222222222222222");

        private static string userName = "test";
        private static TestInMemoryWriter<QuestionnaireStateTracker> questionnaireStateTackerStorage;
        private static TestInMemoryWriter<QuestionnaireChangeRecord> questionnaireChangeRecordStorage;
    }
}
