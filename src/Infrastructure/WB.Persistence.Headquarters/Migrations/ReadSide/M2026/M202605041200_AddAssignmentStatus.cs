using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(202605041200)]
    public class M202605041200_AddAssignmentStatus : Migration
    {
        public override void Up()
        {
            Create.Column("status")
                .OnTable("assignments")
                .AsInt32()
                .NotNullable()
                .WithDefaultValue(0);
        }

        public override void Down()
        {
            Delete.Column("status").FromTable("assignments");
        }
    }
}
