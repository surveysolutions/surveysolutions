using System.ComponentModel;
using System.Data;
using FluentMigrator;
using FluentMigrator.Postgres;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Localizable(false)]
    [Migration(202210181731)]
    public class M202210181731_AddedInvitationCascadingReferanceOnDeleteInterview : ForwardOnlyMigration
    {
        public override void Up()
        {
            Execute.Sql(@"update invitations
                set interviewid = null
                where interviewid in (
	                select i.interviewid 
	                from invitations i
	                left join interviewsummaries s
		                on s.summaryid = i.interviewid 
	                where s.summaryid is null and i.interviewid is not null)");
            
            Create.ForeignKey("fk_invitations_interviewid__interviewsummaries_summaryid")
                .FromTable("invitations").ForeignColumn("interviewid")
                .ToTable("interviewsummaries").PrimaryColumn("summaryid")
                .OnDelete(Rule.SetNull)
                .OnUpdate(Rule.Cascade);
        }
    }
}
