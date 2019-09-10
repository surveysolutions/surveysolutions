using System.Data;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201909091502)]
    public class M201909091502_AddAssignmentsTables : Migration
    {
        const string assignmentsidentifyinganswers = "assignmentsidentifyinganswers";
        const string assignments = "assignments";

        public override void Up()
        {
            Create.Table(assignments)
                .WithColumn("publickey").AsGuid().PrimaryKey().Indexed()
                .WithColumn("id").AsInt32().Unique()
                .WithColumn("responsibleid").AsGuid().NotNullable()
                .WithColumn("quantity").AsInt32().Nullable()
                .WithColumn("archived").AsBoolean().NotNullable()
                .WithColumn("createdatutc").AsDateTime().NotNullable()
                .WithColumn("updatedatutc").AsDateTime().NotNullable()
                .WithColumn("questionnaireid").AsGuid().NotNullable()
                .WithColumn("questionnaireversion").AsInt32().NotNullable()
                .WithColumn("questionnaire").AsString(255).Nullable()
                .WithColumn("answers").AsCustom("jsonb").Nullable()
                .WithColumn("protectedvariables").AsCustom("jsonb").Nullable()
                .WithColumn("receivedbytabletatutc").AsDateTime().Nullable()
                .WithColumn("isaudiorecordingenabled").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("email").AsString().Nullable()
                .WithColumn("password").AsString().Nullable()
                .WithColumn("webmode").AsBoolean().Nullable();

            Create.Table(assignmentsidentifyinganswers)
                .WithColumn("assignmentid").AsGuid().NotNullable()
                .WithColumn("position").AsInt32().NotNullable()
                .WithColumn("questionid").AsGuid().NotNullable()
                .WithColumn("answer").AsString().Nullable()
                .WithColumn("answerasstring").AsString().Nullable()
                .WithColumn("rostervector").AsCustom("integer[]").NotNullable();

            Create.Index(assignmentsidentifyinganswers + "_" + assignments)
                .OnTable(assignmentsidentifyinganswers)
                .OnColumn("assignmentid")
                .Ascending();

            Create.Index().OnTable(assignments).OnColumn("responsibleid");

            Create.ForeignKey(assignments + "_" + assignmentsidentifyinganswers)
                .FromTable(assignmentsidentifyinganswers).ForeignColumn("assignmentid")
                .ToTable(assignments).PrimaryColumn("publickey")
                .OnDelete(Rule.Cascade);

            //MigrateExistedAssignments();
        }

        private void MigrateExistedAssignments()
        {
            Execute.EmbeddedScript(@"WB.Persistence.Headquarters.Migrations.ReadSide.M2019.M201909091502_MigrateAssignments.sql");
        }

        public override void Down()
        {
            Delete.Table(assignmentsidentifyinganswers);
            Delete.Table(assignments);
        }
    }
}
