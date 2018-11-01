using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Users
{
    [Migration(2)]
    [Localizable(false)]
    public class M002_MigrateUsersFromUserDocumentStore : Migration
    {
        public override void Up()
        {
            this.Execute.Sql(@"                
                DO $$
                DECLARE 
	                hasroles BOOLEAN;
	                rec RECORD;
	                profileId integer;
                BEGIN
                    -- Make sure that we not run this migration if users are already migrated
	                SELECT EXISTS(SELECT 1 FROM users.roles ) INTO hasroles;
	
	                IF HASROLES = FALSE THEN
		                INSERT INTO ""users"".""roles"" (""Id"",""Name"") VALUES ('00000000-0000-0000-0000-000000000001','Administrator');
                                INSERT INTO ""users"".""roles"" (""Id"",""Name"") VALUES ('00000000-0000-0000-0000-000000000002','Supervisor');
                                INSERT INTO ""users"".""roles"" (""Id"",""Name"") VALUES ('00000000-0000-0000-0000-000000000004','Interviewer');
                                INSERT INTO ""users"".""roles"" (""Id"",""Name"") VALUES ('00000000-0000-0000-0000-000000000006','Headquarter');
                                INSERT INTO ""users"".""roles"" (""Id"",""Name"") VALUES ('00000000-0000-0000-0000-000000000007','Observer');
                                INSERT INTO ""users"".""roles"" (""Id"",""Name"") VALUES ('00000000-0000-0000-0000-000000000008','ApiUser');

		                FOR rec IN SELECT id, creationdate, email, islockedbyhq, isarchived, islockedbysupervisor, 
				                password, publickey, supervisorid, supervisorname, username, 
				                lastchangedate, deviceid, personname, phonenumber
		                FROM plainstore.userdocuments
		                LOOP
			                IF rec.supervisorid IS NOT NULL THEN
				                INSERT INTO users.userprofiles (""DeviceId"", ""SupervisorId"") Values (rec.deviceid, rec.supervisorid) RETURNING ""Id"" INTO profileid;
                            ELSE 
                                profileId := NULL;
			                END IF;

			                INSERT INTO users.users(
				                ""Id"", 
				                ""UserProfileId"", 
				                ""FullName"", 
				                ""IsArchived"", 
				                ""IsLockedBySupervisor"", 
				                ""IsLockedByHeadquaters"", 
				                ""CreationDate"", 
				                ""PasswordHashSha1"", 
				                ""Email"", 
				                ""EmailConfirmed"", 
				                ""PasswordHash"", 
				                ""SecurityStamp"", 
				                ""PhoneNumber"", 
				                ""PhoneNumberConfirmed"", 
				                ""TwoFactorEnabled"", 
				                ""LockoutEndDateUtc"", 
				                ""LockoutEnabled"", 
				                ""AccessFailedCount"", 
				                ""UserName"")
			                VALUES( 
				                rec.publickey, 
				                profileid,
				                rec.personname,
				                rec.isarchived,
				                rec.islockedbysupervisor,
				                rec.islockedbyhq,
				                rec.creationdate, 
				                rec.password,
				                rec.email,
				                FALSE, -- EmailConfirmed
				                rec.password,
                                uuid_in(md5(random()::text || now()::text)::cstring), -- http://stackoverflow.com/a/21327318/41483
				                rec.phonenumber,
				                FALSE, -- PhoneNumberConfirmed
				                FALSE, -- TwoFactorEnabled
				                NULL,  -- LockoutEndDateUtc
				                FALSE, -- LockoutEnabled
				                0,     -- AccessFailedCount
				                rec.username);			

			                INSERT INTO users.userroles(""UserId"", ""RoleId"")
                            -- as in src\Core\BoundedContexts\Headquarters\WB.Core.BoundedContexts.Headquarters\Views\User\RoleExtensions.cs
                            SELECT rec.publickey as userid, uuid('00000000-0000-0000-0000-00000000000' || roleid)
			                FROM plainstore.roles
			                WHERE userid = rec.id;

		                END LOOP;
	                END IF;	
                END
                $$
            ");
        }

        public override void Down()
        {

        }
    }
}