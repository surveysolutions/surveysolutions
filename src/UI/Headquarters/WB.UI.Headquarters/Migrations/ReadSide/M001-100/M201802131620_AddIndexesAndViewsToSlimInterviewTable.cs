﻿using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.ReadSide
{
    [Migration(201802131620)]
    public class M201802131620_AddIndexesAndViewsToSlimInterviewTable : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"create or replace view readside.interviews_view as 
                select s.interviewid, q.entityid, rostervector, isenabled, isreadonly, invalidvalidations, 
                    asstring, asint, aslong, asdouble, asdatetime, aslist, asintarray, asintmatrix, asgps, asbool, 
                    asyesno, asaudio, asarea, hasflag
                from readside.interviews i
                join readside.interviewsummaries s on s.id = i.interviewid
                join readside.questionnaire_entities q on q.id = i.entityid");

            Execute.Sql(@"CREATE INDEX interviews_interview_id_idx ON readside.interviews USING btree (interviewid) WITH (FILLFACTOR=80)");

            Execute.Sql(@"ALTER TABLE readside.interviews CLUSTER ON interviews_interview_id_idx;");
            Execute.Sql(@"create index interviews_gps_lat_idx  on readside.interviews using btree (((asgps ->> 'Latitude' )::float8))  where asgps is not null;
                          create index interviews_gps_long_idx on readside.interviews using btree (((asgps ->> 'Longitude')::float8)) where asgps is not null;");
            
            Execute.Sql(@"Analyze readside.interviews");
        }

        public override void Down()
        {
            throw new System.NotImplementedException();
        }
    }
}