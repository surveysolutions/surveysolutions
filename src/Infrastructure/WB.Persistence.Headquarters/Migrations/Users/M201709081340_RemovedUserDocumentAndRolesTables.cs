using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Users
{
    [Migration(201709081340)]
    [Localizable(false)]
    public class M201709081340_RemovedUserDocumentAndRolesTables : Migration
    {
        public override void Up()
        {
            RemoveForeignKeyIfPossible("deviceinfos", "FK_deviceinfos_User_userdocuments_id");
            RemoveForeignKeyIfPossible("deviceinfos", "FK_deviceinfos_userid_userdocuments_id");
            
            Execute.Sql($@"DROP TABLE IF EXISTS plainstore.roles CASCADE");
            Execute.Sql($@"DROP TABLE IF EXISTS plainstore.userdocuments CASCADE");
        }

        private void RemoveForeignKeyIfPossible(string table, string constraint, string schema = "plainstore")
        {
            Execute.Sql($@"ALTER TABLE {schema}.{table} DROP CONSTRAINT IF EXISTS {constraint}");
        }

        public override void Down()
        {
            Execute.Sql($@"
                CREATE TABLE plainstore.roles (
	                userid varchar(255) NOT NULL,
	                roleid int4 NULL,
	                CONSTRAINT ""FK_roles_userid_userdocuments_id"" FOREIGN KEY (userid) REFERENCES plainstore.userdocuments(id)
                )
                WITH (
	                OIDS=FALSE
                ) ;
                CREATE INDEX users_roles_fk ON plainstore.roles (userid text_ops)");

            Execute.Sql($@"CREATE TABLE plainstore.userdocuments (
	                id varchar(255) NOT NULL,
	                creationdate timestamp NULL,
	                email text NULL,
	                islockedbyhq bool NULL,
	                isarchived bool NULL,
	                islockedbysupervisor bool NULL,
	                password text NULL,
	                publickey uuid NULL,
	                supervisorid uuid NULL,
	                supervisorname text NULL,
	                username text NULL,
	                lastchangedate timestamp NULL,
	                deviceid text NULL,
	                personname text NULL,
	                phonenumber text NULL,
	                CONSTRAINT ""PK_userdocuments"" PRIMARY KEY (id)
                )
                WITH (
	                OIDS=FALSE
                ) ;
                CREATE INDEX user_creationdate ON plainstore.userdocuments (creationdate timestamp_ops) ;
                CREATE INDEX user_deviceid ON plainstore.userdocuments (deviceid text_ops) ;
                CREATE INDEX user_email ON plainstore.userdocuments (email text_ops) ;
                CREATE INDEX user_personname ON plainstore.userdocuments (personname text_ops) ;
                CREATE INDEX user_publickey ON plainstore.userdocuments (publickey uuid_ops) ;
                CREATE INDEX user_supervisorid ON plainstore.userdocuments (supervisorid uuid_ops) ;
                CREATE UNIQUE INDEX userdocuments_lower_name_password_key ON plainstore.userdocuments (""lower(username)"" text_ops,password text_ops) ;
            ");
        }
    }
}