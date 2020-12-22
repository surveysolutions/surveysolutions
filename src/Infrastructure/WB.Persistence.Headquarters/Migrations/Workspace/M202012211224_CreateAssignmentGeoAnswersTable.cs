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
                .WithColumn("latitude").AsDouble()
                .WithColumn("longitude").AsDouble()
                .WithColumn("timestamp").AsString().Nullable();
        }
    }
}
