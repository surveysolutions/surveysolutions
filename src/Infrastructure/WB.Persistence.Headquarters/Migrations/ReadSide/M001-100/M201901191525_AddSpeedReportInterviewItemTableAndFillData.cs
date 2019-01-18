using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Dapper;
using FluentMigrator;
using Npgsql;
using NpgsqlTypes;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Infrastructure.Native.Storage;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201901191525)]
    public class M201901191525_AddSpeedReportInterviewItemTableAndFillData : Migration
    {
        public override void Up()
        {
            Create.Table("speedreportinterviewitems")
                .WithColumn("interviewid").AsString().PrimaryKey("pk_speedreportinterviewitems_interviewid")
                .WithColumn("questionnaireid").AsGuid().Indexed("idx_speedreportinterviewitems_questionnaireid")
                .WithColumn("questionnaireversion").AsInt64().Indexed("idx_speedreportinterviewitems_questionnaireversion")
                .WithColumn("createddate").AsDateTime()
                .WithColumn("firstanswerdate").AsDateTime().Nullable().Indexed("idx_speedreportinterviewitems_firstanswerdate")
                .WithColumn("interviewerid").AsGuid().Nullable()
                .WithColumn("interviewername").AsString().Nullable()
                .WithColumn("supervisorid").AsGuid().Nullable()
                .WithColumn("supervisorname").AsString().Nullable();


            Execute.Sql(@"
                INSERT INTO readside.speedreportinterviewitems (interviewid, questionnaireid, questionnaireversion, createddate, firstanswerdate, supervisorid, supervisorname, interviewerid, interviewername)
                SELECT isum.summaryid as interviewid, 
		               isum.questionnaireid as questionnaireid, 
		               isum.questionnaireversion as questionnaireversion, 
		               createstat.timestamp as createddate, 
		               firstanswstat.timestamp as firstanswerdate, 
		               firstanswstat.supervisorid, 
		               firstanswstat.supervisorname, 
		               firstanswstat.interviewerid,
		               firstanswstat.interviewername
                FROM readside.InterviewSummaries isum
                  inner join readside.InterviewCommentedStatuses createstat on isum.SummaryId = createstat.InterviewId and createstat.Status = 12 
                  left join readside.InterviewCommentedStatuses firstanswstat on isum.SummaryId = firstanswstat.InterviewId and firstanswstat.Status = 2 
                   and firstanswstat.Position = (select min(fas.Position)
                                        from   readside.InterviewCommentedStatuses fas
                                        where  isum.SummaryId = fas.InterviewId
                                           and fas.Status = 2)	
                ");
        }

        public override void Down()
        {
            Delete.Index("pk_speedreportinterviewitems_interviewid").OnTable("speedreportinterviewitems");
            Delete.Index("idx_speedreportinterviewitems_firstanswerdate").OnTable("speedreportinterviewitems");
            Delete.Table("speedreportinterviewitems");
        }
    }
}
