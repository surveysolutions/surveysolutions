using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Localizable(false)]
    [Migration(202605061200)]
    public class M202605061200_MapBrowseItems_IncreasePKLength : Migration
    {
        public override void Up()
        {
            Execute.Sql("ALTER TABLE mapbrowseitems ALTER COLUMN id TYPE varchar(1000)");
            Execute.Sql("ALTER TABLE usermaps ALTER COLUMN map TYPE varchar(1000)");
        }

        public override void Down()
        {
            Execute.Sql(@"DO $$ BEGIN
                IF EXISTS (SELECT 1 FROM mapbrowseitems WHERE length(id) > 255) THEN
                    RAISE EXCEPTION 'Cannot roll back migration M202605061200: mapbrowseitems.id contains values longer than 255 characters';
                END IF;
            END $$");
            Execute.Sql(@"DO $$ BEGIN
                IF EXISTS (SELECT 1 FROM usermaps WHERE length(map) > 255) THEN
                    RAISE EXCEPTION 'Cannot roll back migration M202605061200: usermaps.map contains values longer than 255 characters';
                END IF;
            END $$");
            Execute.Sql("ALTER TABLE usermaps ALTER COLUMN map TYPE varchar(255)");
            Execute.Sql("ALTER TABLE mapbrowseitems ALTER COLUMN id TYPE varchar(255)");
        }
    }
}
