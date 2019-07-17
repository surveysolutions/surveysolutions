using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201905201003)]
    public class M201905201003_RemoveSpeedReportItems : Migration
    {
        public override void Up()
        {
            Alter.Table("interviewsummaries")
                .AddColumn("createddate").AsDateTime().Nullable()
                .AddColumn("firstanswerdate").AsDateTime().Nullable().Indexed()
                .AddColumn("firstinterviewerid").AsGuid().Nullable()
                .AddColumn("firstinterviewername").AsString().Nullable()
                .AddColumn("firstsupervisorid").AsGuid().Nullable()
                .AddColumn("firstsupervisorname").AsString().Nullable();

            Execute.Sql(@"update readside.interviewsummaries s
                            set
                            firstinterviewername = sr.interviewername,
                            firstsupervisorname = sr.supervisorname,
                            firstinterviewerid = sr.interviewerid,
                            firstsupervisorid = sr.supervisorid,
                            createddate = sr.createddate,
                            firstanswerdate = sr.firstanswerdate
                            from readside.speedreportinterviewitems sr 
                            where sr.interviewid = s.summaryid");

            Delete.Table("speedreportinterviewitems");
        }

        public override void Down()
        {
        }
    }
}
