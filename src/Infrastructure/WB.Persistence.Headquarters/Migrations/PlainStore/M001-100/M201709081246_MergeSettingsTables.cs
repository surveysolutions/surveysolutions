using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201709081246)]
    public class M201709081246_MergeSettingsTables : Migration
    {
        public override void Up()
        {
            this.Execute.Sql(
                $@"CREATE TABLE IF NOT EXISTS plainstore.appsettings (id text PRIMARY KEY, value JSON NOT NULL)");


            //migrate globalnotices
            Execute.Sql(@"
                DO $$
                DECLARE 
                    doesglobalnoticesTableExists integer;
                BEGIN
                    SELECT count(tablename) FROM pg_tables WHERE schemaname = 'plainstore' AND tablename = 'globalnotices' INTO doesglobalnoticesTableExists;
                    IF doesglobalnoticesTableExists > 0 THEN
                        INSERT INTO plainstore.appsettings (id, value) select id, value from plainstore.globalnotices ON CONFLICT (id) DO NOTHING;
                        DROP TABLE plainstore.globalnotices;
                    END IF;
                END
                $$
            ");

            //migrate versioncheckinginfos
            this.Execute.Sql(@"
                DO $$
                DECLARE 
                    doesversioncheckinginfosTableExists integer;
                BEGIN
                    SELECT count(tablename) FROM pg_tables WHERE schemaname = 'plainstore' AND tablename = 'versioncheckinginfos' INTO doesversioncheckinginfosTableExists;
                    IF doesversioncheckinginfosTableExists > 0 THEN
                        INSERT INTO plainstore.appsettings (id, value) select id, value from plainstore.versioncheckinginfos ON CONFLICT (id) DO NOTHING;
                        DROP TABLE plainstore.versioncheckinginfos;
                    END IF;
                END
                $$
            ");
            
            //migrate companylogos
            Execute.Sql(@"
                DO $$
                DECLARE 
                    doescompanylogosTableExists integer;
                BEGIN
                    SELECT count(tablename) FROM pg_tables WHERE schemaname = 'plainstore' AND tablename = 'companylogos' INTO doescompanylogosTableExists;
                    IF doescompanylogosTableExists > 0 THEN
                        INSERT INTO plainstore.appsettings (id, value) select id, value from plainstore.companylogos ON CONFLICT (id) DO NOTHING;
                        DROP TABLE plainstore.companylogos;
                END IF;
                END
                $$");

            //migrate exportencryptionsettings 
            Execute.Sql($@"
                DO $$
                DECLARE 
                    doesexportencryptionsettingsTableExists integer;
                BEGIN
                    SELECT count(tablename) FROM pg_tables WHERE schemaname = 'plainstore' AND tablename = 'exportencryptionsettings' INTO doesexportencryptionsettingsTableExists;
                    IF doesexportencryptionsettingsTableExists > 0 THEN
                        INSERT INTO plainstore.appsettings (id, value) select id, value from plainstore.exportencryptionsettings ON CONFLICT (id) DO NOTHING;
                        DROP TABLE plainstore.exportencryptionsettings;
                    END IF;
                END
                $$");

            //migrate supportedquestionnaireversion 
            Execute.Sql(@"
                DO $$
                DECLARE 
                    doessupportedquestionnaireversionTableExists integer;
                BEGIN
                    SELECT count(tablename) FROM pg_tables WHERE schemaname = 'plainstore' AND tablename = 'supportedquestionnaireversion' INTO doessupportedquestionnaireversionTableExists;
                    IF doessupportedquestionnaireversionTableExists > 0 THEN
                      INSERT INTO plainstore.appsettings (id, value) 
                        SELECT 'QuestionnaireVersion', cast('{""MinQuestionnaireVersionSupportedByInterviewer"":""' || MinQuestionnaireVersionSupportedByInterviewer || '""}' as json) from plainstore.supportedquestionnaireversion ON CONFLICT (id) DO NOTHING;
                        DROP TABLE plainstore.supportedquestionnaireversion;
                    END IF;
                END
                $$");
        }

        public override void Down()
        {
            
        }
    }
}