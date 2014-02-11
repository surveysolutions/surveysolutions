using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Supervisor.EventHandler;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;

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
            interviewExportedDataEventHandler = CreateInterviewExportedDataEventHandlerForQuestionnarieCreatedByMethod(
                () => questionnarie,
                CreateInterviewData, r => result = r);
        };

        Because of = () =>
           interviewExportedDataEventHandler.Handle(CreatePublishableEvent());

        It should_records_count_equals_1 = () =>
            result.Levels[0].Records.Length.ShouldEqual(1);

        It should__first_record_have_2_answers= () =>
           result.Levels[0].Records[0].Questions.Length.ShouldEqual(2);

        It should_first_record_id_equals_0 = () =>
            result.Levels[0].Records[0].RecordId.ShouldEqual(0);

        private static InterviewExportedDataEventHandler interviewExportedDataEventHandler;
        private static InterviewDataExportView result;
        private static QuestionnaireDocument questionnarie;
    }
}
