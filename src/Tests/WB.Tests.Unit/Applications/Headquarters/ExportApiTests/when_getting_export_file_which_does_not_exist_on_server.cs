using System.Web.Http;
using System.Web.Http.Results;
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
    public class when_getting_export_file_which_does_not_exist_on_server : ExportControllerTestsContext
    {
        Establish context = () =>
        {
            var filebasedExportedDataAccessor = Mock.Of<IFilebasedExportedDataAccessor>(
                x => x.GetArchiveFilePathForExportedData(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<DataExportFormat>(),
                    Moq.It.IsAny<InterviewStatus?>()) == "path to export file");

            var fileSystemAccessor = Mock.Of<IFileSystemAccessor>(x => x.IsFileExists(Moq.It.IsAny<string>()) == false);

            controller = CreateExportController(filebasedExportedDataAccessor: filebasedExportedDataAccessor, fileSystemAccessor: fileSystemAccessor);
        };

        Because of = () => result = controller.Get(new QuestionnaireIdentity().ToString(), "spss");

        It should_return_http_not_found_response = () =>
            result.ShouldBeOfExactType<NotFoundResult>();

        private static ExportController controller;

        private static IHttpActionResult result;
    }
}