using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ReadSideRepositoryDataExportWriterTests
{
    internal class when_DataExportWriter_adds_interview_data_records : ReadSideRepositoryDataExportWriterTestContext
    {
        Establish context = () =>
        {
            interviewExportedDataStorage = new TestInMemoryWriter<InterviewExportedDataRecord>();

            interviewHistoryStorage = new TestInMemoryWriter<InterviewHistory>();
            readSideRepositoryDataExportWriter =
                CreateReadSideRepositoryDataExportWriter(
                    interviewExportedDataStorage: interviewExportedDataStorage,
                    interviewActionsDataStorage: interviewHistoryStorage);

            interviewDataExportView = Create.InterviewDataExportView(
                interviewId: interviewId,
                levels: Create.InterviewDataExportLevelView(
                    interviewId,
                    Create.InterviewDataExportRecord(
                        interviewId,
                        Create.ExportedQuestion())));
        };

        Because of = () =>
            readSideRepositoryDataExportWriter.AddOrUpdateInterviewRecords(interviewDataExportView,
                interviewDataExportView.TemplateId, interviewDataExportView.TemplateVersion);

        It should_stored_data_for_the_interview_be_not_null = () =>
            interviewExportedDataStorage.GetById(interviewId).Data.ShouldNotBeNull();

        private static ReadSideRepositoryDataExportWriter readSideRepositoryDataExportWriter;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static TestInMemoryWriter<InterviewExportedDataRecord> interviewExportedDataStorage;
        private static TestInMemoryWriter<InterviewHistory> interviewHistoryStorage;
        private static InterviewDataExportView interviewDataExportView;
    }
}
