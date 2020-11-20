using System.Data;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(202011181332)]
    public class M202011181332_AddColumnsToCalendarEvents : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("calendarevents").Column("starttimezone").Exists())
            {
                Create.Column("starttimezone").OnTable("calendarevents")
                    .AsString().Nullable();
            }
            
            if (!Schema.Table("calendarevents").Column("interviewkey").Exists())
            {
                Create.Column("interviewkey").OnTable("calendarevents")
                    .AsString().Nullable();
            }
        }

        public override void Down()
        {
            Delete.Column("starttimezone").FromTable("calendarevents");   
            Delete.Column("interviewkey").FromTable("calendarevents");   
        }
    }
}
