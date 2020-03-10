using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Text;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(202006031200)]
    public class M202006031200_RemoveDuplicateIndexes : Migration
    {
        public override void Up()
        {
            Delete.Index("questionnairebrowseitems_featuredquestions").OnTable("featuredquestions").InSchema("plainstore");
        }
        public override void Down()
        {
        }
    }
}
