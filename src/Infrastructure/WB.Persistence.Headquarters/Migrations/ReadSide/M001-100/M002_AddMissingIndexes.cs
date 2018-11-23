using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(2)]
    public class M002_AddMissingIndexes : Migration
    {
        private const string schemaName = "readside";

        public override void Up()
        {
            if (!Schema.Table("answerstofeaturedquestions").Index("answerstofeaturedquestions_answervalue").Exists())
            {
                Execute.Sql($"CREATE INDEX answerstofeaturedquestions_answervalue ON {schemaName}.answerstofeaturedquestions (answervalue text_pattern_ops)");
            }

            if (!Schema.Table("interviewdataexportrecords").Index("interviewdataexportrecords_id_text_pattern_ops_idx").Exists())
            {
                Execute.Sql($"CREATE INDEX interviewdataexportrecords_id_text_pattern_ops_idx ON {schemaName}.interviewdataexportrecords(id text_pattern_ops)");
            }
        }

        public override void Down()
        {
            Delete.Index("answerstofeaturedquestions_answervalue");
            Delete.Index("interviewdataexportrecords_id_text_pattern_ops_idx");
        }
    }
}