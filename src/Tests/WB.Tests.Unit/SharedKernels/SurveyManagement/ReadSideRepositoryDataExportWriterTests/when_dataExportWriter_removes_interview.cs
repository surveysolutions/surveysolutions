using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ReadSideRepositoryDataExportWriterTests
{
    internal class when_DataExportWriter_removes_interview : ReadSideRepositoryDataExportWriterTestContext
    {
        Establish context = () =>
        {
            interviewExportedDataStorageMock=new Mock<IReadSideRepositoryWriter<InterviewExportedDataRecord>>();
            interviewHistoryStorageMock = new Mock<IReadSideRepositoryWriter<InterviewHistory>>();
            readSideRepositoryDataExportWriter =
                CreateReadSideRepositoryDataExportWriter(
                    interviewExportedDataStorage: interviewExportedDataStorageMock.Object,
                    interviewActionsDataStorage: interviewHistoryStorageMock.Object);
        };

        Because of = () =>
           readSideRepositoryDataExportWriter.DeleteInterviewRecords(interviewId);

        It should_delete_interview_exported_data = () =>
            interviewExportedDataStorageMock.Verify(x=>x.Remove(interviewId.FormatGuid()), Times.Once);

        It should_delete_interview_actions = () =>
            interviewExportedDataStorageMock.Verify(x => x.Remove(interviewId.FormatGuid()), Times.Once);

        private static ReadSideRepositoryDataExportWriter readSideRepositoryDataExportWriter;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static Mock<IReadSideRepositoryWriter<InterviewExportedDataRecord>> interviewExportedDataStorageMock;
        private static Mock<IReadSideRepositoryWriter<InterviewHistory>> interviewHistoryStorageMock;

    }
}
