using System.Data;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(202010261732)]
    public class M202010261732_AddCalendarEvents : Migration
    {
        public override void Up()
        {
            Create.Table("calendarevents")
                .WithColumn("eventid").AsGuid().PrimaryKey()
                .WithColumn("startutc").AsDateTime().Nullable()
                .WithColumn("comment").AsString().Nullable()
                .WithColumn("updatedateutc").AsDateTime().Nullable()
                .WithColumn("interviewid").AsGuid().Nullable()
                .WithColumn("assignmentid").AsInt32().Nullable()
                .WithColumn("username").AsString().Nullable()
                .WithColumn("userid").AsGuid().Nullable()
                .WithColumn("iscompleted").AsBoolean().Nullable()
                .WithColumn("isdeleted").AsBoolean().Nullable();

            Create.Index().OnTable("calendarevents").OnColumn("interviewid");
            Create.Index().OnTable("calendarevents").OnColumn("assignmentid");
        }

        public override void Down()
        {
            
        }
    }
}
