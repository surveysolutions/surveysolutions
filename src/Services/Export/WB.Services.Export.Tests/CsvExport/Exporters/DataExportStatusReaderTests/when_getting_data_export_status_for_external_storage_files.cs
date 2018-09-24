using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Tests.Unit.DataExportTests.DataExportStatusReaderTests
{
    internal class when_getting_data_export_status_for_external_storage_files : DataExportStatusReaderTestContext
    {
        private DataExportStatusReader dataExportStatusReader;
        private QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.NewGuid(), 1);
        private DateTime LastExportTime = new DateTime(1963, 11, 23);
        private DataExportStatusView result;

        private Mock<IDataExportProcessesService> dataExportProcessesService = new Mock<IDataExportProcessesService>();
        private Mock<IFileSystemAccessor> fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
        private Mock<IFilebasedExportedDataAccessor> filebasedExportedDataAccessor = new Mock<IFilebasedExportedDataAccessor>();
        private Mock<IExternalFileStorage> externalFileStorage = new Mock<IExternalFileStorage>();
        
        [SetUp]
        public void EstablishContext()
        {
            dataExportProcessesService.Setup(x => x.GetRunningExportProcesses())
                .Returns(new DataExportProcessDetails[]
                {
                    //Create.Entity.DataExportProcessDetails(format: DataExportFormat.Binary, questionnaireIdentity: questionnaireIdentity)
                });

            var tabularDataExportFilePath = "tabularDataExportFilePath";

            filebasedExportedDataAccessor.Setup(
                    x => x.GetArchiveFilePathForExportedData(questionnaireIdentity, DataExportFormat.Tabular, null,
                        null, null))
                .Returns(tabularDataExportFilePath);

            externalFileStorage.Setup(efs => efs.IsEnabled()).Returns(true);
            externalFileStorage.Setup(efs => efs.GetObjectMetadata(Moq.It.IsAny<string>()))
                .Returns(new FileObject
                {
                    Path = tabularDataExportFilePath,
                    Size = 100500,
                    LastModified = LastExportTime
                });

            dataExportStatusReader =
                CreateDataExportStatusReader(dataExportProcessesService.Object,
                    fileSystemAccessor: fileSystemAccessorMock.Object,
                    externalFileStorage: externalFileStorage.Object,
                    filebasedExportedDataAccessor: filebasedExportedDataAccessor.Object);

            Because();
        }

        protected void Because()
        {
            result = dataExportStatusReader.GetDataExportStatusForQuestionnaire(questionnaireIdentity);
        }

        [Test]
        public void should_return_that_binary_export_exists()
        {
            Assert.True(DataExportView(DataExportType.Data, DataExportFormat.Binary).HasDataToExport);
        }

        [Test]
        public void should_return_that_binary_export_filesize_got_from_extrnalStorage()
        {
            Assert.That(DataExportView(DataExportType.Data, DataExportFormat.Binary).FileSize, Is.EqualTo(FileSizeUtils.SizeInMegabytes(100500)));
        }

        [Test]
        public void should_return_that_binary_export_lastModifiedDate_got_from_extrnalStorage()
        {
            Assert.That(DataExportView(DataExportType.Data, DataExportFormat.Binary).LastUpdateDate, Is.EqualTo(LastExportTime));
        }

        private DataExportView DataExportView(DataExportType dataExportType, DataExportFormat format)
        {
            return result.DataExports.Single(
                d => d.DataExportType == dataExportType && d.DataExportFormat == format);
        }

    }
}