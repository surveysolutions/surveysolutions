using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.DataExportTests.DataExportStatusReaderTests
{
    internal class when_getting_data_export_status_for_questionnaire : DataExportStatusReaderTestContext
    {
        private DataExportStatusReader dataExportStatusReader;
        private QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.NewGuid(), 1);
        private DataExportStatusView result;

        private Mock<IDataExportProcessesService> dataExportProcessesService = new Mock<IDataExportProcessesService>();
        private Mock<IFileSystemAccessor> fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
        private Mock<IFilebasedExportedDataAccessor> filebasedExportedDataAccessor = new Mock<IFilebasedExportedDataAccessor>();

        protected void Because()
        {
            result = dataExportStatusReader.GetDataExportStatusForQuestionnaire(questionnaireIdentity);
        }

        private DataExportView DataExportView(DataExportType dataExportType, DataExportFormat format)
        {
            return result.DataExports.Single(
                d => d.DataExportType == dataExportType && d.DataExportFormat == format);
        }

        [SetUp]
        public void EstablishContext()
        {
            dataExportProcessesService.Setup(x => x.GetRunningExportProcesses())
                .Returns(new[]
                {
                    Create.Entity.DataExportProcessDetails(format: DataExportFormat.Paradata,
                        questionnaireIdentity: questionnaireIdentity),
                    Create.Entity.DataExportProcessDetails(questionnaireIdentity),
                    Create.Entity.DataExportProcessDetails()
                });

            var tabularDataExportFilePath = "tabularDataExportFilePath";

            filebasedExportedDataAccessor.Setup(
                    x => x.GetArchiveFilePathForExportedData(questionnaireIdentity, DataExportFormat.Tabular, null,
                        null, null))
                .Returns(tabularDataExportFilePath);
            fileSystemAccessorMock.Setup(x => x.IsFileExists(tabularDataExportFilePath)).Returns(true);

            dataExportStatusReader =
                CreateDataExportStatusReader(dataExportProcessesService.Object,
                    fileSystemAccessor: fileSystemAccessorMock.Object,
                    filebasedExportedDataAccessor: filebasedExportedDataAccessor.Object);

            Because();
        }

        [Test]
        public void should_CanRefreshBeRequested_be_false_for_data_tabular_export()
        {
            DataExportView(DataExportType.Data, DataExportFormat.Tabular).CanRefreshBeRequested.Should().BeFalse();
        }

        [Test]
        public void should_CanRefreshBeRequested_be_true_for_data_binary_export()
        {
            DataExportView(DataExportType.Data, DataExportFormat.Binary).CanRefreshBeRequested.Should().BeFalse();
        }

        [Test]
        public void should_CanRefreshBeRequested_be_true_for_data_stata_export()
        {
            DataExportView(DataExportType.Data, DataExportFormat.STATA).CanRefreshBeRequested.Should().BeTrue();
        }

        [Test]
        public void should_export_data_in_tabular_stata_spss_and_binary_formats()
        {
            ShouldExportDataTypeInFormat(DataExportType.Data, DataExportFormat.Tabular, DataExportFormat.STATA,
                DataExportFormat.SPSS, DataExportFormat.Binary);
        }

        [Test]
        public void should_export_para_data()
        {
            ShouldExportDataTypeInFormat(DataExportType.ParaData, DataExportFormat.Paradata, DataExportFormat.Tabular);
        }

        [Test]
        public void should_first_running_process_be_of_type_ParaData()
        {
            result.RunningDataExportProcesses[0].Type.Should().Be(DataExportType.ParaData);
        }

        [Test]
        public void should_HasDataToExport_be_false_for_data_binary_export()
        {
            DataExportView(DataExportType.Data, DataExportFormat.Binary).HasDataToExport.Should().BeFalse();
        }

        [Test]
        public void should_HasDataToExport_be_false_for_data_stata_export()
        {
            DataExportView(DataExportType.Data, DataExportFormat.STATA).HasDataToExport.Should().BeFalse();
        }

        [Test]
        public void should_HasDataToExport_be_true_for_data_tabular_export()
        {
            DataExportView(DataExportType.Data, DataExportFormat.Tabular).HasDataToExport.Should().BeTrue();
        }

        [Test]
        public void should_return_3_running_processes()
        {
            result.RunningDataExportProcesses.Length.Should().Be(3);
        }

        [Test]
        public void should_return_5_data_exports()
        {
            result.DataExports.Length.Should().Be(6);
        }

        [Test]
        public void should_second_running_process_be_of_type_Data()
        {
            result.RunningDataExportProcesses[1].Type.Should().Be(DataExportType.Data);
        }

        [Test]
        public void should_second_running_process_QuestionnaireIdentity_be_equal_to_questionnaireIdentity()
        {
            result.RunningDataExportProcesses[1].QuestionnaireIdentity.Should().Be(questionnaireIdentity);
        }

        [Test]
        public void should_second_running_process_QuestionnaireIdentity_not_be_null()
        {
            result.RunningDataExportProcesses[0].QuestionnaireIdentity.Should().Be(questionnaireIdentity);
        }

        [Test]
        public void should_third_running_process_be_of_type_Data()
        {
            result.RunningDataExportProcesses[2].Type.Should().Be(DataExportType.Data);
        }

        [Test]
        public void should_third_running_process_QuestionnaireIdentity_be_not_equal_to_questionnaireIdentity()
        {
            result.RunningDataExportProcesses[2].QuestionnaireIdentity.Should().NotBe(questionnaireIdentity);
        }

        private void ShouldExportDataTypeInFormat(DataExportType dataExportType,
            params DataExportFormat[] expectedFormats)
        {
            result.DataExports.Where(d => d.DataExportType == dataExportType)
                .Select(d => d.DataExportFormat)
                .ToArray()
                .Should().BeEquivalentTo(expectedFormats);
        }
    }
}
