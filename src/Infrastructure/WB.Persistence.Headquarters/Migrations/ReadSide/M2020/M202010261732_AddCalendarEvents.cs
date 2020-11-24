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
                .WithColumn("startutc").AsDateTime().NotNullable()
                .WithColumn("starttimezone").AsString().Nullable()
                .WithColumn("comment").AsString().Nullable()
                .WithColumn("updatedateutc").AsDateTime().NotNullable()
                .WithColumn("interviewid").AsGuid().Nullable()
                .WithColumn("interviewkey").AsString().Nullable()
                .WithColumn("assignmentid").AsInt32().NotNullable()
                .WithColumn("username").AsString().Nullable()
                .WithColumn("creatoruserid").AsGuid().NotNullable()
                .WithColumn("completedatutc").AsDateTime().Nullable()
                .WithColumn("deletedatutc").AsDateTime().Nullable();

            Create.Index().OnTable("calendarevents").OnColumn("interviewid");
            Create.Index().OnTable("calendarevents").OnColumn("assignmentid");
        }

        public override void Down()
        {
            
        }
    }
}
