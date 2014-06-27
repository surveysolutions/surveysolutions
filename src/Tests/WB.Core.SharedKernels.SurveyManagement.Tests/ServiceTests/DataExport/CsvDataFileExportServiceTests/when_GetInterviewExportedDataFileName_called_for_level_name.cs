using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Services;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.ServiceTests.DataExport.CsvDataFileExportServiceTests
{
    internal class when_GetInterviewExportedDataFileName_called_for_level_name : CsvDataFileExportServiceTestContext
    {
        Establish context = () =>
        {
            csvDataFileExportService = CreateCsvDataFileExportService();
        };

        Because of = () =>
            result = csvDataFileExportService.GetInterviewExportedDataFileName(levelName);

        It should_result_be_equal_to_level_name_csv = () =>
            result.ShouldEqual(levelName+".csv");

        private static CsvDataFileExportService csvDataFileExportService;
        private static readonly string levelName = "level name";
        private static string result;
    }
}
