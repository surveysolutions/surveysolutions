using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(202007140700)]
    public class M202007140700_MoveInterviewGpsAnswersSummaryId : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                    alter table readside.interview_geo_answers drop constraint ""PK_interview_geo_answers_id"";
                    alter table readside.interview_geo_answers drop constraint 
                        ""UC_interview_geo_answers_interviewid_questionid_rostervector"";");

            Execute.Sql(@"
                    create table readside.__interview_geo_answers (
	                    id serial not null,
	                    interview_id int4 not null,
	                    questionid uuid not null,
	                    rostervector text not null,
	                    latitude float8 not null,
	                    longitude float8 not null,
	                    ""timestamp"" text null,
	                    isenabled bool not null default true,
	                    constraint ""PK_interview_geo_answers_id"" primary key (id),
	                    constraint ""UC_interview_geo_answers_interviewid_questionid_rostervector"" 
                            unique (interview_id, questionid, rostervector)
                    );");

            Execute.Sql(@"
                    insert into readside.__interview_geo_answers
                    (interview_id, isenabled, latitude, longitude, questionid, rostervector , ""timestamp"")
                    select s.id as interview_id, iga.isenabled, iga.latitude, iga.longitude, iga.questionid, 
                        iga.rostervector, iga.""timestamp"" 
                    from readside.interview_geo_answers iga 
                    join readside.interviewsummaries s on iga.interviewid = s.summaryid
                    order by iga.id;");
            Execute.Sql(@"
                    drop table readside.interview_geo_answers;
                    alter table readside.__interview_geo_answers rename to interview_geo_answers;");
        }

        public override void Down()
        {
        }
    }
}
