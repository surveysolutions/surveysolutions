using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Users
{
    [Migration(201807241755)]
    [Localizable(false)]
    public class M201807241755_ReplaceEmptyEmailToNull : Migration
    {
        public override void Up()
        {
            Execute.Sql($@"UPDATE users.users SET ""Email"" = NULL WHERE ""Email"" = ''");
        }

        public override void Down()
        {
        }
    }
}
