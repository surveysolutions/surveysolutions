using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Tests.Unit.SharedKernels.SurveyManagement;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireChangeHistoryDenormalizerTests
{
    internal class when_questionnaire_group_is_changing : QuestionnaireChangeHistoryDenormalizerTestContext
    {
        Establish context = () =>
        {
            questionnaireStateTackerStorage =
                Mock.Of<IReadSideKeyValueStorage<QuestionnaireStateTacker>>(
                    _ => _.GetById(Moq.It.IsAny<string>()) == Create.QuestionnaireStateTacker());
            questionnaireChangeRecordStorage = new TestInMemoryWriter<QuestionnaireChangeRecord>();

            questionnaireChangeHistoryDenormalizer =
                CreateQuestionnaireChangeHistoryDenormalizer(questionnaireStateTacker: questionnaireStateTackerStorage,
                    questionnaireChangeRecord: questionnaireChangeRecordStorage);
        };

        Because of = () =>
        {
            questionnaireChangeHistoryDenormalizer.Handle(Create.NewGroupAddedEvent(groupId));
            questionnaireChangeHistoryDenormalizer.Handle(Create.GroupClonedEvent(groupId2));
            questionnaireChangeHistoryDenormalizer.Handle(Create.GroupUpdatedEvent(groupId, groupTitle));
            questionnaireChangeHistoryDenormalizer.Handle(Create.GroupBecameARosterEvent(groupId));
            questionnaireChangeHistoryDenormalizer.Handle(Create.RosterChanged(groupId));
            questionnaireChangeHistoryDenormalizer.Handle(Create.GroupStoppedBeingARosterEvent(groupId));
            questionnaireChangeHistoryDenormalizer.Handle(Create.GroupDeletedEvent(groupId));
        };

        It should_store_7_changes = () =>
            GetAllRecords(questionnaireChangeRecordStorage).Length.ShouldEqual(7);

        It should_store_first_change_record_with_target_type_equal_to_group = () =>
            GetAllRecords(questionnaireChangeRecordStorage)[0].TargetItemType.ShouldEqual(QuestionnaireItemType.Group);

        It should_store_second_change_record_with_target_type_equal_to_group = () =>
            GetAllRecords(questionnaireChangeRecordStorage)[1].TargetItemType.ShouldEqual(QuestionnaireItemType.Group);

        It should_store_third_change_record_with_target_type_equal_to_group = () =>
            GetAllRecords(questionnaireChangeRecordStorage)[2].TargetItemType.ShouldEqual(QuestionnaireItemType.Group);

        It should_store_forth_change_record_with_target_type_equal_to_group = () =>
            GetAllRecords(questionnaireChangeRecordStorage)[3].TargetItemType.ShouldEqual(QuestionnaireItemType.Group);

        It should_store_fifth_change_record_with_target_type_equal_to_group = () =>
            GetAllRecords(questionnaireChangeRecordStorage)[4].TargetItemType.ShouldEqual(QuestionnaireItemType.Roster);

        It should_store_six_change_record_with_target_type_equal_to_group = () =>
            GetAllRecords(questionnaireChangeRecordStorage)[5].TargetItemType.ShouldEqual(QuestionnaireItemType.Roster);

        It should_store_seventh_change_record_with_target_type_equal_to_group = () =>
            GetAllRecords(questionnaireChangeRecordStorage)[6].TargetItemType.ShouldEqual(QuestionnaireItemType.Group);

        private static QuestionnaireChangeHistoryDenormalizer questionnaireChangeHistoryDenormalizer;
        private static string groupId = "11111111111111111111111111111111";
        private static string groupId2 = "22222222222222222222222222222222";

        private static string groupTitle = "test";
        private static IReadSideKeyValueStorage<QuestionnaireStateTacker> questionnaireStateTackerStorage;
        private static TestInMemoryWriter<QuestionnaireChangeRecord> questionnaireChangeRecordStorage;
    }
}
