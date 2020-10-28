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
                .WithColumn("eventid").AsString(255).PrimaryKey()
                .WithColumn("start").AsDateTime().Nullable()
                .WithColumn("comment").AsString().Nullable()
                .WithColumn("updatedate").AsDateTime().Nullable()
                .WithColumn("interviewid").AsGuid().Nullable()
                .WithColumn("assignmentid").AsInt32().Nullable()
                .WithColumn("username").AsString().Nullable()
                .WithColumn("userid").AsGuid().Nullable()
                .WithColumn("iscompleted").AsBoolean().Nullable();
        }

        public override void Down()
        {
            
        }
    }
}
