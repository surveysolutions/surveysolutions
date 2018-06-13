using System;
using System.Web.Http;
using System.Web.Http.Results;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.API.PublicApi;

namespace WB.Tests.Unit.Applications.Headquarters.ExportApiTests
{
    public class when_getting_export_process_details_and_specified_process_does_not_exists : ExportControllerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var mockOfDataExportStatusReader = new Mock<IDataExportStatusReader>();
            mockOfDataExportStatusReader.Setup(x => x.GetDataExportStatusForQuestionnaire(questionnaireIdentity, null, null, null))
                .Returns((DataExportStatusView)null);

            controller = CreateExportController(dataExportStatusReader: mockOfDataExportStatusReader.Object);
            BecauseOf();
        }

        private void BecauseOf() => result = controller.ProcessDetails(questionnaireIdentity.ToString(), DataExportFormat.Tabular);

        [NUnit.Framework.Test] public void should_return_http_not_found_response () =>
            result.Should().BeOfType<NotFoundResult>();

        private static ExportController controller;

        private static readonly Mock<IDataExportProcessesService> mockOfDataExportProcessesService =
            new Mock<IDataExportProcessesService>();

        private static IHttpActionResult result;
        private static readonly QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("11111111111111111111111111111111"), 1);
    }
}
