using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(2)]
    public class M002_AddUsersIndex : Migration
    {
        private const string schemaName = "plainstore";
        private const string usersIndexName = "userdocuments_lower_name_password_key";

        public override void Up()
        {
            Execute.Sql($"create unique index {usersIndexName} on {schemaName}.userdocuments(lower(username), password);");
        }

        public override void Down()
        {
            Delete.Index(usersIndexName);
        }
    }
}