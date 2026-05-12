using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(202605081200)]
    public class M202605081200_AddAssignmentStatusComment : Migration
    {
        public override void Up()
        {
            Create.Column("status_comment")
                .OnTable("assignments")
                .AsString().Nullable();
        }

        public override void Down()
        {
            Delete.Column("status_comment").FromTable("assignments");
        }
    }
}
