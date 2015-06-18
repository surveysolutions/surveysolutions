using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Utils;
using WB.Tests.Unit.SharedKernels.SurveyManagement;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireChangeHistoryDenormalizerTests
{
    class when_questionnaire_is_cloned : QuestionnaireChangeHistoryDenormalizerTestContext
    {
        Establish context = () =>
        {
            questionnaireStateTackerStorage = new TestInMemoryWriter<QuestionnaireStateTracker>();

            var clonedQuestionnaireState = Create.QuestionnaireStateTacker();
            clonedQuestionnaireState.GroupsState.Add(clonedFromQuestionnaireId, "clone");

            questionnaireStateTackerStorage.Store(clonedQuestionnaireState, clonedFromQuestionnaireId.FormatGuid());
            questionnaireChangeRecordStorage = new TestInMemoryWriter<QuestionnaireChangeRecord>();
            questionnaireChangeHistoryDenormalizer = CreateQuestionnaireChangeHistoryDenormalizer(questionnaireStateTacker: questionnaireStateTackerStorage, questionnaireChangeRecord: questionnaireChangeRecordStorage);
        };

        Because of = () =>
            questionnaireChangeHistoryDenormalizer.Handle(Create.QuestionnaireClonedEvent(questionnaireId: questionnaireId, questionnaireTitle: questionnaireTitle, clonedFromQuestionnaireId: clonedFromQuestionnaireId));

        It should_store_change_record_with_target_action_equal_to_clone = () =>
            GetFirstChangeRecord(questionnaireChangeRecordStorage, questionnaireId).ActionType.ShouldEqual(QuestionnaireActionType.Clone);

        It should_store_change_record_with_target_type_equal_to_questionnaire = () =>
            GetFirstChangeRecord(questionnaireChangeRecordStorage, questionnaireId).TargetItemType.ShouldEqual(QuestionnaireItemType.Questionnaire);

        It should_store_change_record_with_target_title_equal_to_test = () =>
            GetFirstChangeRecord(questionnaireChangeRecordStorage, questionnaireId).TargetItemTitle.ShouldEqual(questionnaireTitle);

        It should_store_change_record_with_reference_on_cloned_questionnaire = () =>
           GetFirstChangeRecord(questionnaireChangeRecordStorage, questionnaireId).References.First().ReferenceId.ShouldEqual(clonedFromQuestionnaireId);

        It should_store_questionnaire_title_in_state_tracker = () =>
            questionnaireStateTackerStorage.GetById(questionnaireId).GroupsState[Guid.Parse(questionnaireId)].ShouldEqual(questionnaireTitle);

        private static QuestionnaireChangeHistoryDenormalizer questionnaireChangeHistoryDenormalizer;
        private static string questionnaireId = "11111111111111111111111111111111";
        private static Guid clonedFromQuestionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static string questionnaireTitle = "test";
        private static TestInMemoryWriter<QuestionnaireStateTracker> questionnaireStateTackerStorage;
        private static TestInMemoryWriter<QuestionnaireChangeRecord> questionnaireChangeRecordStorage;
    }
}
