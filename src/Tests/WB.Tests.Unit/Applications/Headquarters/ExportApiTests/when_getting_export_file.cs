using System.Web.Http;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Headquarters.API;
using WB.UI.Headquarters.API.PublicApi;

namespace WB.Tests.Unit.Applications.Headquarters.ExportApiTests
{
    public class when_getting_export_file : ExportControllerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var filebasedExportedDataAccessor = Mock.Of<IFilebasedExportedDataAccessor>(
                x => x.GetArchiveFilePathForExportedData(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<DataExportFormat>(),
                    Moq.It.IsAny<InterviewStatus?>(), null, null) == "path to export file");

            var fileSystemAccessor = Mock.Of<IFileSystemAccessor>(x => x.IsFileExists(Moq.It.IsAny<string>()) == true);

            controller = CreateExportController(filebasedExportedDataAccessor: filebasedExportedDataAccessor, fileSystemAccessor: fileSystemAccessor);
            BecauseOf();
        }

        private void BecauseOf() => result = controller.Get(new QuestionnaireIdentity().ToString(), DataExportFormat.SPSS);

        [NUnit.Framework.Test] public void should_return_progressive_download_result () =>
            result.Should().BeOfType<ProgressiveDownloadResult>();

        private static ExportController controller;

        private static IHttpActionResult result;
    }
}
