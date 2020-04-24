using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Text;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(202004241423)]
    public class M202004241423_AddHasSmallSubstitutionsColumnToInterviewSummary : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Column("hassmallsubstitutions").OnTable("interviewsummaries").AsBoolean().NotNullable().SetExistingRowsTo(false);
        }
    }
}
