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
                .WithColumn("questionnaireid").AsGuid().NotNullable()
                .WithColumn("version").AsInt32().NotNullable()
                .WithColumn("categoriesid").AsGuid().NotNullable()
                .WithColumn("sortindex").AsInt32().NotNullable()
                .WithColumn("parentvalue").AsInt32().Nullable()
                .WithColumn("value").AsInt32().NotNullable()
                .WithColumn("text").AsString().NotNullable()
                ;

//            Create.ForeignKey("reusablecategoricaloptions_questionnairebrowseitems")
//                .FromTable("questionnairebrowseitems").ForeignColumns("")
//                .ToTable("reusablecategoricaloptions").PrimaryColumns("");
        }
    }
}
