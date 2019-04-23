using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(201904111853)]
    public class M201904111853_ChangeHiloToIdentity : Migration
    {
        public override void Up()
        {
            this.Execute.Sql(@"CREATE SEQUENCE plainstore.allowedaddresses_pk;
ALTER TABLE plainstore.allowedaddresses ALTER COLUMN id SET DEFAULT nextval('plainstore.allowedaddresses_pk');
ALTER SEQUENCE plainstore.allowedaddresses_pk OWNED BY plainstore.allowedaddresses.id;
SELECT setval('plainstore.allowedaddresses_pk', COALESCE(max(id), 1)) FROM plainstore.allowedaddresses;
            ");
        }

        public override void Down()
        {
        }
    }
}
