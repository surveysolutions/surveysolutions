using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ReadSideRepositoryDataExportWriterTests
{
    internal class when_dataExportWriter_add_interview_action : ReadSideRepositoryDataExportWriterTestContext
    {
        Establish context = () =>
        {
            interviewExportedDataStorage=new TestInMemoryWriter<InterviewExportedDataRecord>();
            interviewExportedDataStorage.Store(Create.InterviewExportedDataRecord(), interviewId);

            interviewHistoryStorage = new TestInMemoryWriter<InterviewHistory>();
            readSideRepositoryDataExportWriter =
                CreateReadSideRepositoryDataExportWriter(
                    interviewExportedDataStorage: interviewExportedDataStorage,
                    interviewActionsDataStorage: interviewHistoryStorage);

            interviewActionExportView = Create.InterviewActionExportView(interviewId);
        };

        private Because of = () =>
            readSideRepositoryDataExportWriter.AddActionRecord(interviewActionExportView,
                Guid.NewGuid(), 1);

        It should_update_LastAction_for_interview_exported_data = () =>
            interviewExportedDataStorage.GetById(interviewId).LastAction.ShouldEqual(interviewActionExportView.Action);

        It should_add_interview_action_to_interview_history_storage = () =>
            interviewHistoryStorage.GetById(interviewId).InterviewActions.First().Action.ShouldEqual(interviewActionExportView.Action);

        private static ReadSideRepositoryDataExportWriter readSideRepositoryDataExportWriter;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static TestInMemoryWriter<InterviewExportedDataRecord> interviewExportedDataStorage;
        private static TestInMemoryWriter<InterviewHistory> interviewHistoryStorage;
        private static InterviewActionExportView interviewActionExportView;
    }
}
