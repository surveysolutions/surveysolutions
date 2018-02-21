﻿using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.ReadSide
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
	            invalidvalidations int4[] NULL,
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

            Execute.Sql(@"ANALYZE VERBOSE readside.interviewsummaries; ANALYZE VERBOSE readside.questionnaire_entities");

            Execute.Sql(@"
                INSERT INTO readside.temp_interviews (interviewid, entityid, rostervector, isenabled, isreadonly, 
                    invalidvalidations, asstring, asint, aslong, asdouble, asdatetime, aslist, asintarray, asintmatrix, 
                    asgps, asbool, asyesno, asaudio, asarea, hasflag)
                SELECT s.id as interviewid, 
                    q.id as entityid, 
                    array_to_string(i.rostervector,'-') as rostervector, 
                    isenabled, isreadonly, 
                    case when coalesce(array_length(invalidvalidations, 1), 0) = 0 then null else invalidvalidations end as invalidvalidations,
                    asstring, asint, aslong, asdouble, asdatetime, aslist, asintarray, asintmatrix, asgps, asbool, asyesno, asaudio, asarea, hasflag
                from readside.interviews i
                    inner join readside.interviewsummaries s on s.interviewid = i.interviewid 
                    inner join readside.questionnaire_entities q 
                        on q.entityid = i.entityid  and q.questionnaireidentity = s.questionnaireidentity");
            
            //Delete.Table(@"interviews").InSchema(@"readside");
            Rename.Table(@"interviews").InSchema(@"readside").To(@"interviews_old");
            Rename.Table(@"temp_interviews").InSchema(@"readside").To(@"interviews");

        }

        public override void Down()
        {
            throw new System.NotImplementedException();
        }
    }
}