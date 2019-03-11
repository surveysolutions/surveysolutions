using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(16)]
    public class M016_AddQuestionnaireCompilationForOldVersionsTable : Migration
    {
        public override void Up()
        {
            if (!this.Schema.Table("questionnairecompilationversions").Exists())
            {
                this.Create.Table("questionnairecompilationversions")
                    .WithColumn("id").AsGuid().PrimaryKey()
                    .WithColumn("version").AsInt32()
                    .WithColumn("description").AsString().Nullable();
            }
        }

        public override void Down()
        {
            this.Delete.Table("questionnairecompilationversions");
        }
    }
}