using System.Web.Http;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Headquarters.API;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Headquarters.ExportApiTests
{
    public class when_getting_export_file : ExportControllerTestsContext
    {
        Establish context = () =>
        {
            var filebasedExportedDataAccessor = Mock.Of<IFilebasedExportedDataAccessor>(
                x => x.GetArchiveFilePathForExportedData(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<DataExportFormat>(),
                    Moq.It.IsAny<InterviewStatus?>()) == "path to export file");

            var fileSystemAccessor = Mock.Of<IFileSystemAccessor>(x => x.IsFileExists(Moq.It.IsAny<string>()) == true);

            controller = CreateExportController(filebasedExportedDataAccessor: filebasedExportedDataAccessor, fileSystemAccessor: fileSystemAccessor);
        };

        Because of = () => result = controller.Get(new QuestionnaireIdentity().ToString(), "spss");

        It should_return_progressive_download_result = () =>
            result.ShouldBeOfExactType<ProgressiveDownloadResult>();

        private static ExportController controller;

        private static IHttpActionResult result;
    }
}