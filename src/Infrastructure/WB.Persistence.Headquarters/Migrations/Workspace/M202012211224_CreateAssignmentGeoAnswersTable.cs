using System.Data;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Migration(202012211224)]
    public class M202012211224_CreateAssignmentGeoAnswersTable : ForwardOnlyMigration
    {
        private string primaryKeyName = "pk_assignment_geo_answers";
        private string uniqueKeyName = "assignment_geo_answers_unique_idx";
        
        public override void Up()
        {
            Create.Table("assignment_geo_answers")
                .WithColumn("id").AsInt32().PrimaryKey(primaryKeyName).Identity()
                .WithColumn("assignment_id").AsInt32()//.Unique(uniqueKeyName)
                .WithColumn("questionid").AsGuid()//.Unique(uniqueKeyName)
                .WithColumn("rostervector").AsString().Nullable()//.Unique(uniqueKeyName)
                .WithColumn("latitude").AsDouble().Indexed()
                .WithColumn("longitude").AsDouble().Indexed()
                .WithColumn("timestamp").AsString().Nullable();

            Create.UniqueConstraint(uniqueKeyName)
                .OnTable("assignment_geo_answers")
                .Columns("assignment_id", "questionid", "rostervector");
            
            Create.ForeignKey("fk_assignment_geo_answers_to_assignments")
                .FromTable("assignment_geo_answers").ForeignColumn("assignment_id")
                .ToTable("assignments").PrimaryColumn("id")
                .OnDelete(Rule.Cascade);
        }
    }
}
