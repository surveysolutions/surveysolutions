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
    internal class when_questionnaire_static_text_is_changing : QuestionnaireChangeHistoryDenormalizerTestContext
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
            questionnaireChangeHistoryDenormalizer.Handle(Create.StaticTextAddedEvent(staticTextId));
            questionnaireChangeHistoryDenormalizer.Handle(Create.StaticTextUpdatedEvent(staticTextId));
            questionnaireChangeHistoryDenormalizer.Handle(Create.StaticTextClonedEvent(staticTextId2));
            questionnaireChangeHistoryDenormalizer.Handle(Create.StaticTextDeletedEvent(staticTextId));
        };

        It should_store_4_changes = () =>
           GetAllRecords(questionnaireChangeRecordStorage).Length.ShouldEqual(4);

        It should_store_change_all_record_with_target_type_equal_to_static_text = () =>
            GetAllRecords(questionnaireChangeRecordStorage)
                .ShouldEachConformTo(r => r.TargetItemType == QuestionnaireItemType.StaticText);

        private static QuestionnaireChangeHistoryDenormalizer questionnaireChangeHistoryDenormalizer;
        private static string staticTextId = "11111111111111111111111111111111";
        private static string staticTextId2 = "22222222222222222222222222222222";

        private static IReadSideKeyValueStorage<QuestionnaireStateTracker> questionnaireStateTackerStorage;
        private static TestInMemoryWriter<QuestionnaireChangeRecord> questionnaireChangeRecordStorage;
    }
}
