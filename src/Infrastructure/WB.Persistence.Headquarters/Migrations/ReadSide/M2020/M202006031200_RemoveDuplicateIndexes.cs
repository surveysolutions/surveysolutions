using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Text;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(202006031200)]
    public class M202006031200_RemoveDuplicateIndexes : Migration
    {
        public override void Up()
        {
            Delete.Index("questionnaire_entities_questionnaireid_idx").OnTable("questionnaire_entities").InSchema("readside");
        }

        public override void Down()
        {
        }

    }
}
