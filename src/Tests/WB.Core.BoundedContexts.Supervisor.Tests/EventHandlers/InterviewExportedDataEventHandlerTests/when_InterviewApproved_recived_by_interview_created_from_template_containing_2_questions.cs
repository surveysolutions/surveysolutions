﻿using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Supervisor.EventHandler;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.GenericSubdomains.Utils;

namespace WB.Core.BoundedContexts.Supervisor.Tests.EventHandlers.InterviewExportedDataEventHandlerTests
{
    internal class when_InterviewApproved_recived_by_interview_created_from_template_containing_2_questions : InterviewExportedDataEventHandlerTestContext
    {
        Establish context = () =>
        {
            questionnarie = CreateQuestionnaireDocument(new Dictionary<string, Guid>
            {
                { "q1", Guid.NewGuid() },
                { "q2", Guid.NewGuid() }
            });
            interviewExportedDataDenormalizer = CreateInterviewExportedDataEventHandlerForQuestionnarieCreatedByMethod(
                () => questionnarie,
                CreateInterviewData, r => result = r);
        };

        Because of = () =>
           interviewExportedDataDenormalizer.Handle(CreatePublishableEvent());

        It should_records_count_equals_1 = () =>
            result.Levels[0].Records.Length.ShouldEqual(1);

        It should__first_record_have_2_answers= () =>
           result.Levels[0].Records[0].Questions.Length.ShouldEqual(2);

        It should_first_record_id_equals_0 = () =>
            result.Levels[0].Records[0].RecordId.ShouldEqual(result.Levels[0].Records[0].InterviewId.FormatGuid());

        It should_first_parent_id_equals_null = () =>
            result.Levels[0].Records[0].ParentRecordId.ShouldBeNull();

        private static InterviewExportedDataDenormalizer interviewExportedDataDenormalizer;
        private static InterviewDataExportView result;
        private static QuestionnaireDocument questionnarie;
    }
}
