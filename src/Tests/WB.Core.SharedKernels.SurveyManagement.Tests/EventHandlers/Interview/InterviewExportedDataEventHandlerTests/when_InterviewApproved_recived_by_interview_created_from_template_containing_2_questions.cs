using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.Interview.InterviewExportedDataEventHandlerTests
{
    internal class when_InterviewApproved_recived_by_interview_created_from_template_containing_2_questions :
        InterviewExportedDataEventHandlerTestContext
    {
        Establish context = () =>
        {
            questionnarie = CreateQuestionnaireDocument(new Dictionary<string, Guid>
            {
                { "q1", Guid.NewGuid() },
                { "q2", Guid.NewGuid() }
            });
            dataExportServiceMock = CreateDataExportService(r => result = r);
            interviewExportedDataDenormalizer = CreateInterviewExportedDataEventHandlerForQuestionnarieCreatedByMethod(
                templateCreationAction: () => questionnarie, dataExportService: dataExportServiceMock.Object,
                dataCreationAction: CreateInterviewData, userDocument: new UserDocument());
        };

        Because of = () =>
            interviewExportedDataDenormalizer.Handle(CreatePublishableEvent(() => new InterviewApproved(Guid.NewGuid(),"comment")));

        It should_ApproveBySupervisor_action_be_added_to_dataExport = () =>
            dataExportServiceMock.Verify(
                x =>
                    x.AddInterviewAction(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>(),
                        Moq.It.Is<InterviewActionExportView>(view => view.Action == InterviewExportedAction.ApproveBySupervisor)));

        It should_records_count_equals_1 = () =>
            result.Levels[0].Records.Length.ShouldEqual(1);

        It should__first_record_have_2_answers = () =>
            result.Levels[0].Records[0].Questions.Length.ShouldEqual(2);

        It should_first_record_id_equals_0 = () =>
            result.Levels[0].Records[0].RecordId.ShouldEqual(result.Levels[0].Records[0].InterviewId.FormatGuid());

        It should_first_parent_ids_be_empty = () =>
            result.Levels[0].Records[0].ParentRecordIds.ShouldBeEmpty();

        private static InterviewExportedDataDenormalizer interviewExportedDataDenormalizer;
        private static QuestionnaireDocument questionnarie;
        private static Mock<IDataExportService> dataExportServiceMock;
        private static InterviewDataExportView result;
    }
}
