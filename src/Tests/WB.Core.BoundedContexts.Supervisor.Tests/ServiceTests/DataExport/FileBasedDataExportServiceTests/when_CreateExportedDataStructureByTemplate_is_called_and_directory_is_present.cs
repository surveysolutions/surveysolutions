using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Supervisor.Implementation.Services.DataExport;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.Infrastructure.FileSystem;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Supervisor.Tests.ServiceTests.DataExport.FileBasedDataExportServiceTests
{
    internal 
        class when_CreateExportedDataStructureByTemplate_is_called_and_directory_is_present : FileBasedDataExportServiceTestContext
    {
        Establish context = () =>
        {
            var fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(true);
            fileBasedDataExportService = CreateFileBasedDataExportService(fileSystemAccessorMock.Object);
        };

        private Because of = () =>
            raisedException =
                Catch.Exception(() => fileBasedDataExportService.CreateExportedDataStructureByTemplate(new QuestionnaireExportStructure())) as InterviewDataExportException;

        It should_InterviewDataExportException_be_rised = () =>
            raisedException.ShouldNotBeNull();

        It should_rised_exception_has_message_about_present_folder = () =>
            raisedException.Message.ShouldContain("export structure for questionnaire with id");

        private static FileBasedDataExportService fileBasedDataExportService;
        private static InterviewDataExportException raisedException;
    }
}
