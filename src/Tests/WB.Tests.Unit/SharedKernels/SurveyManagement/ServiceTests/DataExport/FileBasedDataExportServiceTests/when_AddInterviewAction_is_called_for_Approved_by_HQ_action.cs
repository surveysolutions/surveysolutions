using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.FileBasedDataExportServiceTests
{
    internal class when_AddInterviewAction_is_called_for_Approved_by_HQ_action : FileBasedDataExportServiceTestContext
    {
        Establish context = () =>
        {
            interviewSummaryWriter = new Mock<IReadSideRepositoryWriter<InterviewSummary>>();
            interviewSummaryWriter.Setup(x => x.GetById(interviewId.FormatGuid())).Returns(new InterviewSummary());

            interviewExportServiceMock = new Mock<IDataExportWriter>();

            fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(true);
            fileSystemAccessorMock.Setup(x => x.MakeValidFileName(Moq.It.IsAny<string>())).Returns("1st");
            fileSystemAccessorMock.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>())).Returns<string, string>(Path.Combine);

            fileBasedDataExportRepositoryWriter = CreateFileBasedDataExportService(fileSystemAccessorMock.Object,
                interviewExportServiceMock.Object, interviewSummaryWriter: interviewSummaryWriter.Object, user: new UserDocument());
        };

        Because of = () =>
            fileBasedDataExportRepositoryWriter.AddInterviewAction(InterviewExportedAction.ApproveByHeadquarter, interviewId, Guid.NewGuid(), DateTime.Now);

        It should_pass_InterviewerAssigned_action_to_data_export_writer = () =>
            interviewExportServiceMock.Verify(
                x => x.AddActionRecord(Moq.It.Is<InterviewActionExportView>(i => i.Action == InterviewExportedAction.ApproveByHeadquarter), Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()), Times.Once);

        It should_delete_All_Data_folder = () =>
            fileSystemAccessorMock.Verify(
                x => x.DeleteDirectory("AllData"), Times.Once);

        It should_delete_Approved_Data_folder = () =>
           fileSystemAccessorMock.Verify(
               x => x.DeleteDirectory("ApprovedData"), Times.Once);

        private static FileBasedDataExportRepositoryWriter fileBasedDataExportRepositoryWriter;

        private static Mock<IDataExportWriter> interviewExportServiceMock;
        private static Mock<IReadSideRepositoryWriter<InterviewSummary>> interviewSummaryWriter;
        private static Guid interviewId = Guid.NewGuid();
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
    }
}
