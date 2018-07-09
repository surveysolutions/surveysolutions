using System.ComponentModel;
using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.ReadSide
{
    [Migration(201806191240)]
    [Localizable(false)]
    public class M201806191240_FillErrorsCountInInterviewSummary : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                with counts as (
	                select summ.summaryid as id, count(i.interviewid) as errorscount
	                from readside.interviews i
	                join readside.interviews_id iv on i.interviewid = iv.id
	                join readside.interviewsummaries summ on summ.interviewid = iv.interviewid
	                where i.isenabled and i.invalidvalidations != '' and i.invalidvalidations is not null
	                group by summ.summaryid
                ) 
                UPDATE readside.interviewsummaries
                SET errorscount = counts.errorscount
                from counts where summaryid = counts.id
            ");
        }

        public override void Down()
        {

        }
    }
}
