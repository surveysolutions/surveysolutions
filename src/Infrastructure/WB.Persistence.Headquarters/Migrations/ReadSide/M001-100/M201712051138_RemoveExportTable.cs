using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201712051138)]
    [Localizable(false)]
    public class M201712051138_RemoveExportTable : Migration
    {
        public override void Up()
        {
            Delete.Table("interviewdataexportrecords");
        }

        public override void Down()
        {
            Create.Table("interviewdataexportrecords")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("recordid").AsString().Nullable()
                .WithColumn("interviewid").AsGuid().Nullable()
                .WithColumn("levelname").AsString().Nullable()
                .WithColumn("parentrecordids").AsCustom("text[]").Nullable()
                .WithColumn("referencevalues").AsCustom("text[]").Nullable()
                .WithColumn("systemvariablevalues").AsCustom("text[]").Nullable()
                .WithColumn("answers").AsCustom("text[]").Nullable();

            Execute.Sql("CREATE INDEX interviewdataexportrecords_id_text_pattern_ops_idx ON readside.interviewdataexportrecords(id text_pattern_ops)");
        }
    }
}