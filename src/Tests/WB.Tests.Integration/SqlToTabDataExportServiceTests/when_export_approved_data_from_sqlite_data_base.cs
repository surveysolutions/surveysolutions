using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Services.Sql;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Tests.Integration.SqlToTabDataExportServiceTests
{
    internal class when_export_approved_data_from_sqlite_data_base : SqlToTabDataExportServiceTestContext
    {
        Establish context = () =>
        {
            csvWriterService = new CsvWriterServiceTest();
            sqlServiceFactory = Create.SqliteServiceFactory("sqllite_export_test");

            questionnaireExportStructure =
                Create.QuestionnaireExportStructure(Create.HeaderStructureForLevel(rosterLevelTable,
                    referenceNames: new[] { "1", "2" },
                    levelScopeVector: new ValueVector<Guid>(new[] { Guid.NewGuid(), Guid.NewGuid() })));

            sqlToTabDataExportService = CreateSqlToTabDataExportService(questionnaireExportStructure: questionnaireExportStructure, csvWriterService: csvWriterService, sqlServiceFactory: sqlServiceFactory);


            RunCommand(sqlServiceFactory,
                string.Format("insert into [{0}] values ('{1}', @param);", rosterLevelTable, interviewId),
                new
                {
                    param =
                        Encoding.Unicode.GetBytes(string.Join(ExportFileSettings.SeparatorOfExportedDataFile.ToString(),
                            new[] { interviewId, "r1", "r2", "a1", "a2", "p1", "p2" }))
                });

            RunCommand(sqlServiceFactory,
               string.Format("insert into [{0}] values ('{1}', 'ApproveByHeadquarter', 'nastya', 'inter', 'some data', 'some time');", "interview_actions", interviewId));
        };

        Because of = () =>
            sqlToTabDataExportService.GetDataFilesForQuestionnaireByInterviewsInApprovedState(questionnaireId, questionnaireVersion, "");

        It should_write_roster_level_header_to_data_file = () =>
            csvWriterService.WrittenData[0].ShouldEqual(new[]
            {
                "Id",
                "1",
                "2",
                "1",
                "a",
                "ParentId1",
                "ParentId2"
            });

        It should_write_roster_level_data_to_data_file = () =>
            csvWriterService.WrittenData[1].ShouldEqual(new[]
            {
                interviewId,
                "r1",
                "r2",
                "a1",
                "a2",
                "p1",
                "p2"
            });

        It should_write_interview_actions_header_to_data_file = () =>
            csvWriterService.WrittenData[2].ShouldEqual(new[]
            {
                "InterviewId",
                "Action",
                "Originator",
                "Role",
                "Date",
                "Time"

            });

        It should_write_interview_actions_data_to_data_file = () =>
            csvWriterService.WrittenData[3].ShouldEqual(new[]
            {
                "interview_id_formated",
                "ApproveByHeadquarter",
                "nastya",
                "inter",
                "some data",
                "some time"
            });

        private static SqlToTabDataExportService sqlToTabDataExportService;
        private static CsvWriterServiceTest csvWriterService;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static long questionnaireVersion = 3;
        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static string rosterLevelTable = "roster level table";
        private static ISqlServiceFactory sqlServiceFactory;

        private static string interviewId = "interview_id_formated";
    }
}
