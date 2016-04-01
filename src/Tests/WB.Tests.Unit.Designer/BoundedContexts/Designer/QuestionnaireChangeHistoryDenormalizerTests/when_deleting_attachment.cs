using System;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireChangeHistoryDenormalizerTests
{
    internal class when_deleting_attachment : QuestionnaireChangeHistoryDenormalizerTestContext
    {
        Establish context = () =>
        {
            questionnaireStateTackerStorage = Setup.QuestionnaireStateTrackerForOneQuestionnaire();

            questionnaireChangeHistoryDenormalizer = CreateQuestionnaireChangeHistoryDenormalizer(
                questionnaireStateTacker: questionnaireStateTackerStorage,
                questionnaireChangeRecord: questionnaireChangeRecordStorage);
        };

        Because of = () =>
            questionnaireChangeHistoryDenormalizer.Handle(Create.Event.AttachmentDeleted(questionnaireId: questionnaireId, entityId: attachmentId));

        It should_create_single_history_record = () =>
        {
            var historyRecord = questionnaireChangeRecordStorage.Dictionary.Values.Single();
            historyRecord.ActionType.ShouldEqual(QuestionnaireActionType.Delete);
            historyRecord.QuestionnaireId.ShouldEqual(questionnaireId.FormatGuid());
            historyRecord.TargetItemId.ShouldEqual(attachmentId);
            historyRecord.TargetItemType.ShouldEqual(QuestionnaireItemType.Attachment);
        };

        private static QuestionnaireChangeHistoryDenormalizer questionnaireChangeHistoryDenormalizer;
        private static readonly Guid attachmentId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

        private static IReadSideKeyValueStorage<QuestionnaireStateTracker> questionnaireStateTackerStorage;
        private static readonly TestInMemoryWriter<QuestionnaireChangeRecord> questionnaireChangeRecordStorage = new TestInMemoryWriter<QuestionnaireChangeRecord>();
    }
}