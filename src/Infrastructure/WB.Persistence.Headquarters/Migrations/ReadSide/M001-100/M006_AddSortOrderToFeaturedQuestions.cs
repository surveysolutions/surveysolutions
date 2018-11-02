using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(6)]
    public class M006_AddSortOrderToFeaturedQuestions : Migration
    {
        public override void Up()
        {
            Alter.Table("answerstofeaturedquestions").AddColumn("position").AsInt32().SetExistingRowsTo(-1).NotNullable();
        }

        public override void Down()
        {
            Delete.Column("position").FromTable("answerstofeaturedquestions");
        }
    }
}