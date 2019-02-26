using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201902191529)]
    [Localizable(false)]
    public class M201902191529_AddInvitationsTable : Migration
    {
        const string invitations = "invitations";

        public override void Up()
        {
            Create.Table(invitations)
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("assignmentid").AsInt32().NotNullable()
                .WithColumn("interviewid").AsGuid().Nullable()
                .WithColumn("token").AsString().Nullable()
                .WithColumn("resumepassword").AsString().Nullable()
                .WithColumn("sentonutc").AsDateTime().Nullable()
                .WithColumn("invitationemailid").AsString().Nullable()
                .WithColumn("lastremindersentonutc").AsDateTime().Nullable()
                .WithColumn("lastreminderemailid").AsString().Nullable()
                .WithColumn("numberofreminderssent").AsString().Nullable();
        }

        public override void Down()
        {
            Delete.Table(invitations);
        }
    }
}
