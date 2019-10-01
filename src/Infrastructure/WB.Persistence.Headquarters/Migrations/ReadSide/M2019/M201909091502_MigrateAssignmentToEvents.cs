using System.Data;
using System.Threading;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201909091502)]
    public class M201909091502_MigrateAssignmentToEvents : Migration
    {
        const string assignmentsidentifyinganswers = "assignmentsidentifyinganswers";
        const string assignments = "assignments";

        public override void Up()
        {
            CreateAssignmentTables();

            MigrateExistedAssignments();

            CreateSequenceForAssignmentId();
        }

        private void CreateAssignmentTables()
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
                .WithColumn("audiorecording").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("email").AsString().Nullable()
                .WithColumn("password").AsString().Nullable()
                .WithColumn("webmode").AsBoolean().Nullable()
                .WithColumn("comments").AsString().Nullable();

            Create.Table(assignmentsidentifyinganswers)
                .WithColumn("assignmentid").AsInt32().NotNullable()
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
                .ToTable(assignments).PrimaryColumn("id")
                .OnDelete(Rule.Cascade);
        }

        private void MigrateExistedAssignments()
        {
            if (this.Schema.Schema("plainstore").Table(assignments).Exists()
                && this.Schema.Schema("events").Table("events").Exists()
                && this.Schema.Schema("users").Table("users").Exists())
            {
                Execute.EmbeddedScript(@"WB.Persistence.Headquarters.Migrations.ReadSide.M2019.M201909091502_MigrateAssignments.sql");
            }
        }

        private void CreateSequenceForAssignmentId()
        {
            if (this.Schema.Schema("plainstore").Exists())
            {
                Execute.Sql("CREATE SEQUENCE IF NOT EXISTS plainstore.assignment_id_sequence; "
                          + "SELECT setval('plainstore.assignment_id_sequence', COALESCE(max(id), 1)) FROM readside.assignments; ");
            }
        }

        public override void Down()
        {
            Delete.Table(assignmentsidentifyinganswers);
            Delete.Table(assignments);
        }
    }
}
