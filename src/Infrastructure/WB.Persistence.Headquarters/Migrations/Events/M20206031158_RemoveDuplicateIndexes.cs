using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Text;

namespace WB.Persistence.Headquarters.Migrations.Events
{
    [Migration(202006031158)]
    public class M202006031158_RemoveDuplicateIndexes : Migration
    {
        public override void Up()
        {
            Delete.Index("event_source_indx").OnTable("events").InSchema("events");
        }

        public override void Down()
        {
        }
    }
}
