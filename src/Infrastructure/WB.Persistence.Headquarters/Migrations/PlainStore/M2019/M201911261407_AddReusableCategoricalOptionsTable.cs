using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201911261407)]
    public class M201911261407_AddReusableCategoricalOptionsTable : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("reusablecategoricaloptions")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("questionnaireid").AsGuid()
                .WithColumn("questionnaireversion").AsInt32()
                .WithColumn("categoriesid").AsGuid()
                .WithColumn("sortindex").AsInt32().NotNullable()
                .WithColumn("parentvalue").AsInt32().Nullable()
                .WithColumn("value").AsInt32().NotNullable()
                .WithColumn("text").AsString().NotNullable()
                ;

            Create.Index("idx_categories_reusablecategoricaloptions").OnTable("reusablecategoricaloptions")
                .OnColumn("categoriesid").Ascending()
                .OnColumn("questionnaireid").Ascending()
                .OnColumn("questionnaireversion").Ascending();
            Create.Index("idx_sortindex_reusablecategoricaloptions").OnTable("reusablecategoricaloptions")
                .OnColumn("sortindex");
        }
    }
}
