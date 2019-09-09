using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201909091502)]
    public class M201909091502_AddAssignmentsTables : Migration
    {
        const string assignmentsprefilledanswers = "assignmentsprefilledanswers";
        const string assignments = "assignments";

        public override void Up()
        {
            Create.Table(assignments)
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("responsibleid").AsGuid().NotNullable()
                .WithColumn("capacity").AsInt32().Nullable()
                .WithColumn("archived").AsBoolean().NotNullable()
                .WithColumn("createdatutc").AsDateTime().NotNullable()
                .WithColumn("updatedatutc").AsDateTime().NotNullable()
                .WithColumn("questionnaireid").AsGuid().NotNullable()
                .WithColumn("questionnaireversion").AsInt32().NotNullable();

            Create.Table(assignmentsprefilledanswers)
                .WithColumn("assignmentid").AsInt32().NotNullable()
                .WithColumn("position").AsInt32().NotNullable()
                .WithColumn("questionid").AsGuid().NotNullable()
                .WithColumn("answer").AsString().Nullable();

            Create.Index("assignmentsprefilledanswers_assignments")
                .OnTable(assignmentsprefilledanswers)
                .OnColumn("assignmentid")
                .Ascending();

            MigrateExistedAssignments();
        }

        private void MigrateExistedAssignments()
        {
            Execute.EmbeddedScript(@"WB.Persistence.Headquarters.Migrations.PlainStore.M2019.M201909091502_MigrateAssignments.sql");
        }

        public override void Down()
        {
            Delete.Table(assignmentsprefilledanswers);
            Delete.Table(assignments);
        }
    }
}
