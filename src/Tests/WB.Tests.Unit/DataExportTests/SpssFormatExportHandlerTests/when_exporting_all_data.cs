using System;
using System.Threading;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers;
using WB.Core.BoundedContexts.Headquarters.Services.Export;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.DataExportTests.SpssFormatExportHandlerTests
{
    internal class when_exporting_all_data : SpssFormatExportHandlerTestContext
    {
        Establish context = () =>   
        {
            spssFormatExportHandler = CreateSpssFormatExportHandler(tabularFormatExportService: tabularFormatExportServiceMock.Object);
        };

        Because of = () => spssFormatExportHandler.ExportData(Create.Entity.DataExportProcessDetails());

        It should_export_all_data =
            () =>
                tabularFormatExportServiceMock.Verify(
                    x =>
                        x.ExportInterviewsInTabularFormat(Moq.It.IsAny<QuestionnaireIdentity>(),
                            Moq.It.IsAny<InterviewStatus?>(), Moq.It.IsAny<string>(), Moq.It.IsAny<IProgress<int>>(), Moq.It.IsAny<CancellationToken>()), Times.Once);

        private static Mock<ITabularFormatExportService> tabularFormatExportServiceMock = new Mock<ITabularFormatExportService>();
        private static SpssFormatExportHandler spssFormatExportHandler;
    }
}