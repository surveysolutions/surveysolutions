using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Migration(2020_10_26_17_32)]
    public class M202010261732_AddCalendarEvents : AutoReversingMigration
    {
        public override void Up()
        {
            if (!Schema.Table("calendarevents").Exists())
            {
                Create.Table("calendarevents")
                    .WithColumn("event_id").AsGuid().PrimaryKey()
                    .WithColumn("start_ticks").AsInt64().NotNullable()
                    .WithColumn("start_timezone").AsString().Nullable()
                    .WithColumn("comment").AsString().Nullable()
                    .WithColumn("update_date_utc").AsDateTime().NotNullable()
                    .WithColumn("interview_id").AsGuid().Nullable()
                    .WithColumn("interview_key").AsString().Nullable()
                    .WithColumn("assignment_id").AsInt32().NotNullable()
                    .WithColumn("creator_user_id").AsGuid().NotNullable()
                    .WithColumn("completed_at_utc").AsDateTime().Nullable()
                    .WithColumn("deleted_at_utc").AsDateTime().Nullable();

                Create.Index().OnTable("calendarevents").OnColumn("interview_id");
                Create.Index().OnTable("calendarevents").OnColumn("assignment_id");
            }
        }
    }
}
