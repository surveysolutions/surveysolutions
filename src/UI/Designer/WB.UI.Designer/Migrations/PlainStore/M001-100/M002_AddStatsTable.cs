using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(2)]
    public class M002_AddStatsTable : Migration
    {
        public override void Up()
        {
            Create.Table("questionnaireimportedentries")
                .WithColumn("importdateutc").AsDateTime().PrimaryKey()
                .WithColumn("questionnaireid").AsGuid().NotNullable()
                .WithColumn("supportedbyhqversion").AsCustom("integer[]").NotNullable();
        }

        public override void Down()
        {
            Delete.Table("questionnaireimportedentries");
        }
    }
}