using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore.M2019
{
    [Migration(201904011401)]
    public class M201904011401_AddIndexesOnInvitationTable : Migration
    {
        const string invitations = "invitations";
        public override void Up()
        {
            this.Create.Index($"{invitations}_assignmentid").OnTable(invitations).OnColumn("assignmentid");
            this.Create.Index($"{invitations}_interviewid").OnTable(invitations).OnColumn("interviewid");
            this.Create.Index($"{invitations}_token").OnTable(invitations).OnColumn("token");
            this.Create.Index($"{invitations}_sentonutc").OnTable(invitations).OnColumn("sentonutc");
            this.Create.Index($"{invitations}_lastremindersentonutc").OnTable(invitations).OnColumn("lastremindersentonutc");
        }

        public override void Down()
        {
            this.Delete.Index($"{invitations}_assignmentid");
            this.Delete.Index($"{invitations}_interviewid");
            this.Delete.Index($"{invitations}_token");
            this.Delete.Index($"{invitations}_sentonutc");
            this.Delete.Index($"{invitations}_lastremindersentonutc");
        }
    }
}
