using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireChangeHistoryDenormalizerTests
{
    internal class when_questionnaire_question_is_changing : QuestionnaireChangeHistoryDenormalizerTestContext
    {
        Establish context = () =>
        {
            questionnaireStateTackerStorage =
                Mock.Of<IReadSideKeyValueStorage<QuestionnaireStateTracker>>(
                    _ => _.GetById(Moq.It.IsAny<string>()) == Create.QuestionnaireStateTacker());
            questionnaireChangeRecordStorage = new TestInMemoryWriter<QuestionnaireChangeRecord>();

            questionnaireChangeHistoryDenormalizer =
                CreateQuestionnaireChangeHistoryDenormalizer(questionnaireStateTacker: questionnaireStateTackerStorage,
                    questionnaireChangeRecord: questionnaireChangeRecordStorage);
        };

        Because of = () =>
        {
            questionnaireChangeHistoryDenormalizer.Handle(Create.Event.NewQuestionAddedEvent(questionId));
            questionnaireChangeHistoryDenormalizer.Handle(Create.Event.QuestionClonedEvent(questionId2));
            questionnaireChangeHistoryDenormalizer.Handle(Create.Event.QuestionChangedEvent(questionId: questionId,questionTitle: questionTitle));
            questionnaireChangeHistoryDenormalizer.Handle(Create.Event.QuestionDeletedEvent(questionId));

            questionnaireChangeHistoryDenormalizer.Handle(Create.Event.NumericQuestionClonedEvent(questionId2));
            questionnaireChangeHistoryDenormalizer.Handle(Create.Event.NumericQuestionChangedEvent(questionId));

            questionnaireChangeHistoryDenormalizer.Handle(Create.Event.TextListQuestionClonedEvent(questionId2));
            questionnaireChangeHistoryDenormalizer.Handle(Create.Event.TextListQuestionChangedEvent(questionId));

            questionnaireChangeHistoryDenormalizer.Handle(Create.Event.QRBarcodeQuestionClonedEvent(questionId2));
            questionnaireChangeHistoryDenormalizer.Handle(Create.Event.QRBarcodeQuestionUpdatedEvent(questionId));

            questionnaireChangeHistoryDenormalizer.Handle(Create.Event.MultimediaQuestionUpdatedEvent(questionId));
        };

        It should_store_11_changes = () =>
           GetAllRecords(questionnaireChangeRecordStorage).Length.ShouldEqual(11);

        It should_store_change_all_record_with_target_type_equal_to_group = () =>
            GetAllRecords(questionnaireChangeRecordStorage)
                .ShouldEachConformTo(r => r.TargetItemType == QuestionnaireItemType.Question);

        private static QuestionnaireChangeHistoryDenormalizer questionnaireChangeHistoryDenormalizer;
        private static string questionId = "11111111111111111111111111111111";
        private static string questionId2 = "22222222222222222222222222222222";

        private static string questionTitle = "test";
        private static IReadSideKeyValueStorage<QuestionnaireStateTracker> questionnaireStateTackerStorage;
        private static TestInMemoryWriter<QuestionnaireChangeRecord> questionnaireChangeRecordStorage;
    }
}
