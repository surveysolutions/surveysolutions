using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(202001151349, "Fixed translations for old categories", BreakingChange = false)]
    public class M202001151349_FixedTranslationsForCategories : Migration
    {
        public override void Up()
        {
            Execute.Sql("UPDATE plainstore.translationinstances SET translationindex = translationindex || '$' WHERE type = 4 AND translationindex NOT LIKE '%$%';");
        }

        public override void Down()
        {
            
        }
    }
}
