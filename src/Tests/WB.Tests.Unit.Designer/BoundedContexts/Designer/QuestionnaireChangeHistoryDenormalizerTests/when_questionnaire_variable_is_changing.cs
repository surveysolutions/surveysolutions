using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireChangeHistoryDenormalizerTests
{
    internal class when_questionnaire_variable_is_changing : QuestionnaireChangeHistoryDenormalizerTestContext
    {
        Establish context = () =>
        {
            questionnaireStateTackerStorage = Mock.Of<IReadSideKeyValueStorage<QuestionnaireStateTracker>>(
                                                        _ => _.GetById(Moq.It.IsAny<string>()) == Create.QuestionnaireStateTacker());

            questionnaireChangeRecordStorage = new TestInMemoryWriter<QuestionnaireChangeRecord>();

            questionnaireChangeHistoryDenormalizer = CreateQuestionnaireChangeHistoryDenormalizer(
                                                        questionnaireStateTacker: questionnaireStateTackerStorage,
                                                        questionnaireChangeRecord: questionnaireChangeRecordStorage);
        };

        Because of = () =>
        {
            questionnaireChangeHistoryDenormalizer.Handle(Create.Event.ToPublishedEvent(Create.Event.VariableAdded(variableId)));
            questionnaireChangeHistoryDenormalizer.Handle(Create.Event.ToPublishedEvent(Create.Event.VariableUpdated(variableId)));
            questionnaireChangeHistoryDenormalizer.Handle(Create.Event.ToPublishedEvent(Create.Event.VariableCloned(variableId2)));
            questionnaireChangeHistoryDenormalizer.Handle(Create.Event.ToPublishedEvent(Create.Event.VariableDeleted(variableId)));
        };

        It should_store_4_changes = () =>
           GetAllRecords(questionnaireChangeRecordStorage).Length.ShouldEqual(4);

        It should_store_change_all_record_with_target_type_equal_to_variable = () =>
            GetAllRecords(questionnaireChangeRecordStorage).ShouldEachConformTo(r => r.TargetItemType == QuestionnaireItemType.Variable);

        It should_store_change_record_with_with_add_variable = () =>
            GetAllRecords(questionnaireChangeRecordStorage)[0].ActionType.ShouldEqual(QuestionnaireActionType.Add);

        It should_store_change_record_with_with_update_variable = () =>
            GetAllRecords(questionnaireChangeRecordStorage)[1].ActionType.ShouldEqual(QuestionnaireActionType.Update);

        It should_store_change_record_with_with_clone_variable = () =>
            GetAllRecords(questionnaireChangeRecordStorage)[2].ActionType.ShouldEqual(QuestionnaireActionType.Clone);

        It should_store_change_record_with_with_delete_variable = () =>
            GetAllRecords(questionnaireChangeRecordStorage)[3].ActionType.ShouldEqual(QuestionnaireActionType.Delete);


        private static QuestionnaireChangeHistoryDenormalizer questionnaireChangeHistoryDenormalizer;
        private static Guid variableId = Guid.Parse("11111111111111111111111111111111");
        private static Guid variableId2 = Guid.Parse("22222222222222222222222222222222");

        private static IReadSideKeyValueStorage<QuestionnaireStateTracker> questionnaireStateTackerStorage;
        private static TestInMemoryWriter<QuestionnaireChangeRecord> questionnaireChangeRecordStorage;
    }
}
