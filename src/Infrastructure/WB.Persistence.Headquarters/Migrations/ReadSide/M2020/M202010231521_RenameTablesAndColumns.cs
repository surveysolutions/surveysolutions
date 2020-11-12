using System.Data;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(202010231521)]
    public class M202010231521_RenameTablesAndColumns : AutoReversingMigration
    {
        public override void Up()
        {
            this.Rename.Column("question_id").OnTable("answerstofeaturedquestions").InSchema("readside")
                .To("entity_id");
            this.Rename.Column("answer_lower_case").OnTable("answerstofeaturedquestions").InSchema("readside")
                .To("value_lower_case");
            this.Rename.Column("answervalue").OnTable("answerstofeaturedquestions").InSchema("readside")
                .To("value");
            
            this.Rename.Table("answerstofeaturedquestions").InSchema("readside")
                .To("identifyingentityvalue");
        }
    }
}
