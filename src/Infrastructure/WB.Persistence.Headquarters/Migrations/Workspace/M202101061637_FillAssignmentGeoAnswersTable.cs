using System.Data;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Migration(202101061637)]
    public class M202101061637_FillAssignmentGeoAnswersTable : ForwardOnlyMigration
    {
        public override void Up()
        {
            Execute.Sql($@"insert into assignment_geo_answers (assignment_id, questionid, latitude, longitude)
                                select aia.assignmentId, 
	                                   aia.questionid, 
	                                   cast(split_part(left(aia.answer, strpos(aia.answer, '[') - 1),',',1) as float), 
	                                   cast(split_part(left(aia.answer, strpos(aia.answer, '[') - 1),',',2) as float)
                                from assignments as ass
                                left join assignmentsidentifyinganswers as aia 
                                on ass.id = aia.assignmentid 
                                left join questionnaire_entities as qe
                                on ass.questionnaire = qe.questionnaireidentity
                                where aia.questionid = qe.entityid
                                and aia.answer is not null
                                and featured is not null
                                and qe.entity_type = 2
                                and question_type = 6;");
        }
    }
}
