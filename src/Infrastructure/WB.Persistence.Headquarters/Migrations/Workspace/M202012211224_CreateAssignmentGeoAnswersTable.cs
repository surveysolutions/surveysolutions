using System.Data;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Migration(202012211224)]
    public class M202012211224_CreateAssignmentGeoAnswersTable : ForwardOnlyMigration
    {
        private string primaryKeyName = "pk_assignment_geo_answers";
        
        public override void Up()
        {
            Create.Table("assignment_geo_answers")
                .WithColumn("assignment_id").AsInt32().PrimaryKey(primaryKeyName)
                .WithColumn("questionid").AsGuid().PrimaryKey(primaryKeyName)
                .WithColumn("rostervector").AsString().Nullable().PrimaryKey(primaryKeyName)
                .WithColumn("latitude").AsDouble().Indexed()
                .WithColumn("longitude").AsDouble().Indexed()
                .WithColumn("timestamp").AsString().Nullable();
            
            Create.ForeignKey("fk_assignment_geo_answers_to_assignments")
                .FromTable("assignment_geo_answers").ForeignColumn("assignment_id")
                .ToTable("assignments").PrimaryColumn("id")
                .OnDelete(Rule.Cascade);
        }
    }
}
