using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Migration(202102201432)]
    public class M202102201432_AddColumnsToIdentifyEntityValue : ForwardOnlyMigration
    {
        public override void Up()
        {
            Create.Column("value_date")
                .OnTable("identifyingentityvalue")
                .AsDateTime()
                .Nullable();

            Create.Column("value_double")
                .OnTable("identifyingentityvalue")
                .AsDouble()
                .Nullable();

            Create.Column("value_long")
                .OnTable("identifyingentityvalue")
                .AsInt64()
                .Nullable();

            Create.Column("value_bool")
                .OnTable("identifyingentityvalue")
                .AsBoolean()
                .Nullable();

            Create.Column("enabled")
                .OnTable("identifyingentityvalue")
                .AsBoolean()
                .Nullable();

            Create.Column("identifying")
                .OnTable("identifyingentityvalue")
                .AsBoolean()
                .Nullable()
                .WithDefaultValue(false);

            Update.Table("identifyingentityvalue")
                .Set(new {identifying = true}).AllRows();

            Create.Index().OnTable("identifyingentityvalue").OnColumn("value_date");
            Create.Index().OnTable("identifyingentityvalue").OnColumn("value_double");
            Create.Index().OnTable("identifyingentityvalue").OnColumn("value_long");
        }
    }
}
