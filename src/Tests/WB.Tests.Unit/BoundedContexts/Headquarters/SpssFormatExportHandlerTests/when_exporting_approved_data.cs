using System;
using System.Threading;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.SpssFormatExportHandlerTests
{
    internal class when_exporting_approved_data : SpssFormatExportHandlerTestContext
    {
        Establish context = () =>
        {
            spssFormatExportHandler = CreateSpssFormatExportHandler(tabularFormatExportService: tabularFormatExportServiceMock.Object);
        };

        Because of = () => spssFormatExportHandler.ExportData(Create.ApprovedDataExportProcess());

        It should_export_approved_data_only =
            () =>
                tabularFormatExportServiceMock.Verify(
                    x =>
                        x.ExportApprovedInterviewsInTabularFormat(Moq.It.IsAny<QuestionnaireIdentity>(),
                            Moq.It.IsAny<string>(), Moq.It.IsAny<IProgress<int>>(), Moq.It.IsAny<CancellationToken>()), Times.Once);

        private static Mock<ITabularFormatExportService> tabularFormatExportServiceMock=new Mock<ITabularFormatExportService>();
        private static SpssFormatExportHandler spssFormatExportHandler;
    }
}