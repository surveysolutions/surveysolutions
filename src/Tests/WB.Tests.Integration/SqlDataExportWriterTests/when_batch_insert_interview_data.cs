using System;
using System.Collections.Generic;
using Machine.Specifications;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Sql;
using WB.Core.SharedKernels.SurveyManagement.Services.Sql;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Tests.Integration.SqlDataExportWriterTests
{
    internal class when_batch_insert_interview_data : SqlDataExportWriterTestContext
    {
        Establish context = () =>
        {
            sqliteServiceFactory = Create.SqliteServiceFactoryForTests("sqllite_batch_upload_test");
            interviewDataExportView = CreateInterviewDataExportView(interviewId: interviewId,
                levels: CreateInterviewDataExportLevelView(interviewId: interviewId, levelName: rosterLevelTable,
                    levelVector: new ValueVector<Guid>(new[] { Guid.NewGuid(), Guid.NewGuid() }),
                    records: new[]
                    {
                        CreateInterviewDataExportRecord(interviewId, "0", new[] { "r1", "r2" }, new[] { "1", interviewId.FormatGuid() }),
                        CreateInterviewDataExportRecord(interviewId, "1", new[] { "r3", "r4" }, new[] { "2", interviewId.FormatGuid() })
                    }));

            sqlDataExportWriter = CreateSqlDataExportWriter(sqliteServiceFactory);
            sqlDataExportWriter.CreateStructure(
                Create.QuestionnaireExportStructure(Create.HeaderStructureForLevel(rosterLevelTable, referenceNames: new[] { "1", "2" },
                    levelScopeVector: new ValueVector<Guid>(new[] { Guid.NewGuid(), Guid.NewGuid() }))), "");

            RunCommand(sqliteServiceFactory, string.Format("insert into [interview_actions] values('{0}','Funny status','user','super role','some data','some time')", interviewIdForDelete.FormatGuid()));
            RunCommand(sqliteServiceFactory, string.Format("insert into [interview_actions] values('{0}','Funny status of present interview','user','super role','some data','some time')", interviewId.FormatGuid()));
            
            RunCommand(sqliteServiceFactory, string.Format("insert into [roster level table] values('{0}','somethe data of deleted interview')", interviewIdForDelete.FormatGuid()));
            RunCommand(sqliteServiceFactory, string.Format("insert into [roster level table] values('{0}','somethe data of updates interview')", interviewId.FormatGuid()));
        };

        Because of = () =>
        {
            sqlDataExportWriter.BatchInsert("", new[] { interviewDataExportView },
                new[]
                {
                    new InterviewActionExportView(interviewId.FormatGuid(), InterviewExportedAction.ApproveByHeadquarter, "nastya",
                        actionDate, "int")
                }, new[] { interviewIdForDelete });
            interviewActionsTableData = QueryTo2DimensionStringArray(sqliteServiceFactory, "select * from [interview_actions]");
            interviewRosterLevelTableData = QueryTo2DimensionStringArray(sqliteServiceFactory, "select * from [roster level table]");
        };

        It should_contain_two_actions_at_interview_action_table = () =>
            interviewActionsTableData.Length.ShouldEqual(2);

        It should_contains_old_action_at_first_row_of_interview_actions_table_by_updated_interview = () =>
            interviewActionsTableData[0].ShouldEqual(
                new[]
                {
                    interviewId.FormatGuid(), 
                    "Funny status of present interview",
                    "user",
                    "super role",
                    "some data",
                    "some time"
                }
            );

        It should_contains_new_action_at_second_row_of_interview_actions_table_by_updated_interview = () =>
            interviewActionsTableData[1].ShouldEqual(
                new[]
                {
                    interviewId.FormatGuid(), 
                    "ApproveByHeadquarter",
                    "nastya",
                    "int",
                    "04/18/1984",
                    "06:15:02"
                }
            );

        It should_contain_two_rows_at_roster_level_table = () =>
            interviewRosterLevelTableData.Length.ShouldEqual(2);

        It should_contain_first_row_in_roster_level_table_coresponding_to_first_roster_record = () =>
            interviewRosterLevelTableData[0].ShouldEqual(new[] { interviewId.FormatGuid(), "0	r1	r2	a	1	1	11111111111111111111111111111111" });

        It should_contain_first_row_in_roster_level_table_coresponding_to_second_roster_record = () =>
            interviewRosterLevelTableData[1].ShouldEqual(new[] { interviewId.FormatGuid(), "1	r3	r4	a	1	2	11111111111111111111111111111111" });

        private static SqlDataExportWriter sqlDataExportWriter;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static Guid interviewIdForDelete = Guid.Parse("22222222222222222222222222222222");
        private static InterviewDataExportView interviewDataExportView;
        private static string rosterLevelTable="roster level table";
        private static DateTime actionDate=new DateTime(1984,4,18,6,15,2);

        private static string[][] interviewActionsTableData;
        private static string[][] interviewRosterLevelTableData;

        private static ISqlServiceFactory sqliteServiceFactory;
    }
}
