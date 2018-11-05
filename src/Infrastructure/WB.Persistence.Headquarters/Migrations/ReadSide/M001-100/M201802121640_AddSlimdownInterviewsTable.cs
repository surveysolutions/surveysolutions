using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201802121640)]
    public class M201802121640_AddSlimdownInterviewsTable :Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
            CREATE TABLE readside.temp_interviews (
	            interviewid int4 NOT NULL,
	            entityid int4 NOT NULL,
	            rostervector text NULL,
	            isenabled bool NOT NULL DEFAULT false,
	            isreadonly bool NOT NULL DEFAULT false,
	            invalidvalidations text NULL,
                warnings text null,
	            asstring text NULL,
	            asint int4 NULL,
	            aslong int8 NULL,
	            asdouble float8 NULL,
	            asdatetime timestamp NULL,
	            aslist jsonb NULL,
	            asintarray int4[] NULL,
	            asintmatrix jsonb NULL,
	            asgps jsonb NULL,
	            asbool bool NULL,
	            asyesno jsonb NULL,
	            asaudio jsonb NULL,
	            asarea jsonb NULL,
	            hasflag bool NOT NULL DEFAULT false);");
            
            Execute.Sql(@"ALTER TABLE readside.interviews DROP CONSTRAINT IF EXISTS interviews_pk;");
            Execute.Sql(@"DROP INDEX if exists readside.interviews_asgps_not_null_indx;");

            Execute.Sql($@"
                INSERT INTO readside.temp_interviews (interviewid, entityid, rostervector, isenabled, isreadonly, 
                    invalidvalidations, asstring, asint, aslong, asdouble, asdatetime, aslist, asintarray, asintmatrix, 
                    asgps, asbool, asyesno, asaudio, asarea, hasflag)
                SELECT s.id as interviewid, 
                    q.id as entityid, 
                    array_to_string(i.rostervector,'-') as rostervector, 
                    isenabled, isreadonly, 
                    array_to_string(invalidvalidations,'-') as invalidvalidations, 
                    asstring, asint, aslong, asdouble, asdatetime, aslist, asintarray, asintmatrix, asgps, asbool, asyesno, asaudio, asarea, hasflag
                from readside.interviews i
                    inner join readside.interviews_id s on s.interviewid = i.interviewid
                    inner join readside.interviewsummaries sum on sum.interviewid = i.interviewid
                    inner join readside.questionnaire_entities q 
                        on q.entityid = i.entityid  and q.questionnaireidentity = sum.questionnaireidentity
                where not (q.entity_type = {(int)EntityType.Section})");
            
            Delete.Table(@"interviews").InSchema(@"readside");
            // Rename.Table(@"interviews").InSchema(@"readside").To(@"interviews_old");
            Rename.Table(@"temp_interviews").InSchema(@"readside").To(@"interviews");
        }

        private enum EntityType
        {
            Section = 1,
            Question = 2,
            StaticText = 3,
            Variable = 4
        }

        public override void Down()
        {
        }
    }
}
