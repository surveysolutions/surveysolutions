using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.ServiceTests.DataExport.CsvDataFileExportServiceTests
{
    internal class when_GetInterviewActionFileName_is_called : CsvDataFileExportServiceTestContext
    {
        Establish context = () =>
        {
            csvDataFileExportService = CreateCsvDataFileExportService();
        };

        Because of = () =>
            result = csvDataFileExportService.GetInterviewActionFileName();

        It should_result_be_equal_to_interview_actions_csv = () =>
            result.ShouldEqual("interview_actions.csv");

        private static CsvDataFileExportService csvDataFileExportService;
        private static string result;
    }
}
