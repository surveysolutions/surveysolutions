using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Tests.Unit.SharedKernels.SurveyManagement;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireChangeHistoryDenormalizerTests
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
            questionnaireChangeHistoryDenormalizer.Handle(Create.NewQuestionAddedEvent(questionId));
            questionnaireChangeHistoryDenormalizer.Handle(Create.QuestionClonedEvent(questionId2));
            questionnaireChangeHistoryDenormalizer.Handle(Create.QuestionChangedEvent(questionId: questionId,questionTitle: questionTitle));
            questionnaireChangeHistoryDenormalizer.Handle(Create.QuestionDeletedEvent(questionId));

            questionnaireChangeHistoryDenormalizer.Handle(Create.NumericQuestionAddedEvent(questionId));
            questionnaireChangeHistoryDenormalizer.Handle(Create.NumericQuestionClonedEvent(questionId2));
            questionnaireChangeHistoryDenormalizer.Handle(Create.NumericQuestionChangedEvent(questionId));

            questionnaireChangeHistoryDenormalizer.Handle(Create.TextListQuestionAddedEvent(questionId));
            questionnaireChangeHistoryDenormalizer.Handle(Create.TextListQuestionClonedEvent(questionId2));
            questionnaireChangeHistoryDenormalizer.Handle(Create.TextListQuestionChangedEvent(questionId));

            questionnaireChangeHistoryDenormalizer.Handle(Create.QRBarcodeQuestionAddedEvent(questionId));
            questionnaireChangeHistoryDenormalizer.Handle(Create.QRBarcodeQuestionClonedEvent(questionId2));
            questionnaireChangeHistoryDenormalizer.Handle(Create.QRBarcodeQuestionUpdatedEvent(questionId));

            questionnaireChangeHistoryDenormalizer.Handle(Create.MultimediaQuestionUpdatedEvent(questionId));
        };

        It should_store_14_changes = () =>
           GetAllRecords(questionnaireChangeRecordStorage).Length.ShouldEqual(14);

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
