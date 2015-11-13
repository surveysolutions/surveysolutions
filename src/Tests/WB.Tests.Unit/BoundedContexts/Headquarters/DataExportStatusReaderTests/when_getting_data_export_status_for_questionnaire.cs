﻿using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportProcess;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.DataExportStatusReaderTests
{
    internal class when_getting_data_export_status_for_questionnaire: DataExportStatusReaderTestContext
    {
        Establish context = () =>
        {
            dataExportProcessesService.Setup(x => x.GetRunningProcess())
                .Returns(new IDataExportProcess[]
                {Create.ParaDataExportProcess(), Create.AllDataExportProcess(questionnaireIdentity: questionnaireIdentity), Create.ApprovedDataExportProcess()});

            var tabularDataExportFilePath = "tabularDataExportFilePath";

            filebasedExportedDataAccessor.Setup(
                x => x.GetArchiveFilePathForExportedData(questionnaireIdentity, DataExportFormat.Tabular))
                .Returns(tabularDataExportFilePath);
            fileSystemAccessorMock.Setup(x => x.IsFileExists(tabularDataExportFilePath)).Returns(true);

            dataExportStatusReader =
                CreateDataExportStatusReader(dataExportProcessesService: dataExportProcessesService.Object,
                    fileSystemAccessor: fileSystemAccessorMock.Object,
                    filebasedExportedDataAccessor: filebasedExportedDataAccessor.Object);
        };

        Because of = () => result= dataExportStatusReader.GetDataExportStatusForQuestionnaire(questionnaireIdentity);

        It should_return_3_running_processes = () => result.RunningDataExportProcesses.Length.ShouldEqual(3);

        It should_first_running_process_be_of_type_ParaData = () => result.RunningDataExportProcesses[0].Type.ShouldEqual(DataExportType.ParaData);

        It should_second_running_process_QuestionnaireIdentity_be_null = () => result.RunningDataExportProcesses[0].QuestionnaireIdentity.ShouldBeNull();

        It should_second_running_process_be_of_type_Data = () => result.RunningDataExportProcesses[1].Type.ShouldEqual(DataExportType.Data);

        It should_second_running_process_QuestionnaireIdentity_be_equal_to_questionnaireIdentity = () => result.RunningDataExportProcesses[1].QuestionnaireIdentity.ShouldEqual(questionnaireIdentity);

        It should_third_running_process_be_of_type_Data = () => result.RunningDataExportProcesses[2].Type.ShouldEqual(DataExportType.ApprovedData);

        It should_third_running_process_QuestionnaireIdentity_be_not_equal_to_questionnaireIdentity = () => result.RunningDataExportProcesses[2].QuestionnaireIdentity.ShouldNotEqual(questionnaireIdentity);

        It should_export_para_data_in_tabular_format = () => ShouldExportDataTypeInFormat(DataExportType.ParaData, DataExportFormat.Tabular);

        It should_return_8_data_exports =() => result.DataExports.Length.ShouldEqual(8);

        It should_export_data_in_tabular_stata_spss_and_binary_formats = () => ShouldExportDataTypeInFormat(DataExportType.Data, DataExportFormat.Tabular, DataExportFormat.STATA, DataExportFormat.SPSS, DataExportFormat.Binary);

        It should_export_approved_data_in_tabular_stata_and_spss_formats = () => ShouldExportDataTypeInFormat(DataExportType.ApprovedData, DataExportFormat.Tabular, DataExportFormat.STATA, DataExportFormat.SPSS);

        It should_CanRefreshBeRequested_be_false_for_data_tabular_export = () => DataExportView(DataExportType.Data, DataExportFormat.Tabular).CanRefreshBeRequested.ShouldBeFalse();

        It should_HasDataToExport_be_true_for_data_tabular_export = () => DataExportView(DataExportType.Data, DataExportFormat.Tabular).HasDataToExport.ShouldBeTrue();

        It should_CanRefreshBeRequested_be_true_for_data_stata_export = () => DataExportView(DataExportType.Data, DataExportFormat.STATA).CanRefreshBeRequested.ShouldBeTrue();

        It should_HasDataToExport_be_false_for_data_stata_export = () => DataExportView(DataExportType.Data, DataExportFormat.STATA).HasDataToExport.ShouldBeFalse();

        It should_CanRefreshBeRequested_be_true_for_data_binary_export = () => DataExportView(DataExportType.Data, DataExportFormat.Binary).CanRefreshBeRequested.ShouldBeFalse();

        It should_HasDataToExport_be_false_for_data_binary_export = () => DataExportView(DataExportType.Data, DataExportFormat.Binary).HasDataToExport.ShouldBeFalse();

        private static DataExportStatusReader dataExportStatusReader;
        private static QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.NewGuid(), 1);
        private static DataExportStatusView result;

        private static Mock<IDataExportProcessesService> dataExportProcessesService =
            new Mock<IDataExportProcessesService>();

        private static Mock<IFileSystemAccessor> fileSystemAccessorMock=new Mock<IFileSystemAccessor>();

        private static Mock<IFilebasedExportedDataAccessor> filebasedExportedDataAccessor = new Mock<IFilebasedExportedDataAccessor>();

        private static void ShouldExportDataTypeInFormat(DataExportType dataExportType, params DataExportFormat[] expectedFormats)
        {
            result.DataExports.Where(d => d.DataExportType == dataExportType)
                .Select(d => d.DataExportFormat)
                .ToArray()
                .ShouldEqual(expectedFormats);
        }

        private static DataExportView DataExportView(DataExportType dataExportType, DataExportFormat format)
        {
            return result.DataExports.Single(
                d => d.DataExportType == dataExportType && d.DataExportFormat == format);
        }
    }
}