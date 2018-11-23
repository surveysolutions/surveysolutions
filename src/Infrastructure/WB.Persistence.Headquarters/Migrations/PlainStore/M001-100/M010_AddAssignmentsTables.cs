using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(10)]
    public class M010_AddAssignmentsTables : Migration
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
        }

        public override void Down()
        {
            Delete.Table(assignmentsprefilledanswers);
            Delete.Table(assignments);
        }
    }
}