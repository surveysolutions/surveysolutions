using System;
using System.Threading;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers.Implementation;
using WB.Core.BoundedContexts.Headquarters.Services.Export;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.DataExportTests.SpssFormatExportHandlerTests
{
    public class when_exporting_all_data : SpssFormatExportHandlerTestContext
    {
        [SetUp]
        public void EstablishСontext() 
        {
            spssFormatExportHandler = CreateSpssFormatExportHandler(tabularFormatExportService: tabularFormatExportServiceMock.Object);

            // act
            spssFormatExportHandler.ExportData(Create.Entity.DataExportProcessDetails());
        }

        [Test]
        public void should_export_all_data() =>
                tabularFormatExportServiceMock.Verify(
                    x =>
                        x.ExportInterviewsInTabularFormat(Moq.It.IsAny<QuestionnaireIdentity>(),
                            Moq.It.IsAny<InterviewStatus?>(), Moq.It.IsAny<string>(), Moq.It.IsAny<IProgress<int>>(), Moq.It.IsAny<CancellationToken>(), Moq.It.IsAny<DateTime?>(), Moq.It.IsAny<DateTime?>()), Times.Once);

        private static Mock<ITabularFormatExportService> tabularFormatExportServiceMock = new Mock<ITabularFormatExportService>();
        private static SpssFormatExportHandler spssFormatExportHandler;
    }
}
