﻿using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireChangeHistoryDenormalizerTests
{
    internal class when_new_questionnaire_created : QuestionnaireChangeHistoryDenormalizerTestContext
    {
        Establish context = () =>
        {
            questionnaireStateTackerStorage=new TestInMemoryWriter<QuestionnaireStateTracker>();
            questionnaireChangeRecordStorage=new TestInMemoryWriter<QuestionnaireChangeRecord>();
            questionnaireChangeHistoryDenormalizer = CreateQuestionnaireChangeHistoryDenormalizer(questionnaireStateTacker: questionnaireStateTackerStorage, questionnaireChangeRecord: questionnaireChangeRecordStorage);
        };

        Because of = () =>
            questionnaireChangeHistoryDenormalizer.Handle(Create.Event.NewQuestionnaireCreatedEvent(questionnaireId, questionnaireTitle,true));

        It should_store_change_record_with_target_action_equal_to_add = () =>
            GetFirstChangeRecord(questionnaireChangeRecordStorage, questionnaireId).ActionType.ShouldEqual(QuestionnaireActionType.Add);

        It should_store_change_record_with_target_type_equal_to_questionnaire = () =>
            GetFirstChangeRecord(questionnaireChangeRecordStorage, questionnaireId).TargetItemType.ShouldEqual(QuestionnaireItemType.Questionnaire);

        It should_store_change_record_with_target_title_equal_to_test = () =>
            GetFirstChangeRecord(questionnaireChangeRecordStorage, questionnaireId).TargetItemTitle.ShouldEqual(questionnaireTitle);

        It should_store_questionnaire_title_in_state_tracker = () =>
            questionnaireStateTackerStorage.GetById(questionnaireId).GroupsState[Guid.Parse(questionnaireId)].ShouldEqual(questionnaireTitle);

        private static QuestionnaireChangeHistoryDenormalizer questionnaireChangeHistoryDenormalizer;
        private static string questionnaireId = "11111111111111111111111111111111";
        private static string questionnaireTitle = "test";
        private static TestInMemoryWriter<QuestionnaireStateTracker> questionnaireStateTackerStorage;
        private static TestInMemoryWriter<QuestionnaireChangeRecord> questionnaireChangeRecordStorage;
    }
}
