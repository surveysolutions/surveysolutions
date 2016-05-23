using FluentMigrator;
using Npgsql;

namespace WB.UI.Headquarters.Migrations.ReadSide
{
    [Migration(1)]
    public class M001_InitDb : Migration
    {
        public override void Up()
        {
            Create.Table("cumulativereportstatuschanges")
                .WithColumn("entryid").AsString(255).NotNullable().PrimaryKey()
                .WithColumn("questionnaireid").AsGuid()
                .WithColumn("questionnaireversion").AsInt32()
                .WithColumn("date").AsDateTime()
                .WithColumn("status").AsInt32()
                .WithColumn("changevalue").AsInt32();

            Create.Table("interviewcommentaries")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("isdeleted").AsBoolean()
                .WithColumn("isapprovedbyhq").AsBoolean()
                .WithColumn("questionnaireid").AsString()
                .WithColumn("questionnaireversion").AsInt32();

            Create.Table("commentaries")
                .WithColumn("interviewid").AsString(255).PrimaryKey()
                .WithColumn("commentsequence").AsInt32()
                .WithColumn("originatorname").AsString()
                .WithColumn("originatoruserid").AsGuid()
                .WithColumn("originatorrole").AsInt32()
                .WithColumn("timestamp").AsDateTime()
                .WithColumn("variable").AsString()
                .WithColumn("roster").AsString()
                .WithColumn("rostervector").AsCustom("numeric[]")
                .WithColumn("comment").AsString()
                .WithColumn("position").AsInt32().NotNullable();

            Create.Table("interviewdataexportrecords")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("recordid").AsString()
                .WithColumn("interviewid").AsGuid()
                .WithColumn("levelname").AsString()
                .WithColumn("parentrecordids").AsCustom("text[]")
                .WithColumn("referencevalues").AsCustom("text[]")
                .WithColumn("systemvariablevalues").AsCustom("text[]")
                .WithColumn("answers").AsCustom("text[]");

            Create.Table("mapreportpoints")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("questionnaireid").AsGuid()
                .WithColumn("questionnaireversion").AsInt32()
                .WithColumn("variable").AsString()
                .WithColumn("interviewid").AsGuid()
                .WithColumn("latitude").AsFloat()
                .WithColumn("longitude").AsFloat();

            Create.Table("interviewfeedentries")
                .WithColumn("entryid").AsString(255).PrimaryKey()
                .WithColumn("supervisorid").AsString()
                .WithColumn("entrytype").AsInt32()
                .WithColumn("timestamp").AsDateTime()
                .WithColumn("interviewid").AsString()
                .WithColumn("userid").AsString()
                .WithColumn("interviewerid").AsString();

            Create.Table("interviewresponsibles")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("interviewid").AsGuid()
                .WithColumn("userid").AsGuid();

            Create.Table("interviewstatuses")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("questionnaireid").AsGuid()
                .WithColumn("questionnaireversion").AsInt32();


            Create.Table("interviewcommentedstatuses")
                .WithColumn("interviewid").AsString(255).PrimaryKey()
                .WithColumn("id").AsGuid()
                .WithColumn("supervisorid").AsGuid()
                .WithColumn("supervisorname").AsString()
                .WithColumn("statuschangeoriginatorid").AsGuid()
                .WithColumn("statuschangeoriginatorname").AsString()
                .WithColumn("statuschangeoriginatorrole").AsInt32()
                .WithColumn("status").AsInt32()
                .WithColumn("timestamp").AsDateTime()
                .WithColumn("timespanwithpreviousstatus").AsInt32()
                .WithColumn("comment").AsString()
                .WithColumn("position").AsInt32().NotNullable();

            Create.Table("interviewstatustimespans")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("questionnaireid").AsGuid()
                .WithColumn("questionnaireversion").AsInt32();

            Create.Table("interviewsummaries")
                .WithColumn("summaryid").AsString(255).PrimaryKey()
                .WithColumn("interviewid").AsGuid()
                .WithColumn("questionnairetitle").AsString()
                .WithColumn("responsiblename").AsString()
                .WithColumn("teamleadid").AsGuid()
                .WithColumn("teamleadname").AsString()
                .WithColumn("responsiblerole").AsInt32()
                .WithColumn("updatedate").AsDateTime()
                .WithColumn("wasrejectedbysupervisor").AsBoolean()
                .WithColumn("wascreatedonclient").AsBoolean()
                .WithColumn("receivedbyinterviewer").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("questionnaireid").AsGuid()
                .WithColumn("questionnaireversion").AsInt32()
                .WithColumn("responsibleid").AsGuid()
                .WithColumn("status").AsInt32()
                .WithColumn("isdeleted").AsBoolean()
                .WithColumn("haserrors").AsBoolean();

            Create.Table("answerstofeaturedquestions")
                .WithColumn("id").AsInt32().PrimaryKey()
                .WithColumn("questionid").AsGuid()
                .WithColumn("answertitle").AsString()
                .WithColumn("answervalue").AsString()
                .WithColumn("interviewsummaryid").AsString(255);

            Create.Table("interviewsyncpackagemetas")
                .WithColumn("packageid").AsString(255).PrimaryKey()
                .WithColumn("sortindex").AsInt64()
                .WithColumn("interviewid").AsGuid()
                .WithColumn("itemtype").AsString()
                .WithColumn("serializedpackagesize").AsInt32();

            Create.Table("questionnairefeedentries")
                .WithColumn("entryid").AsString(255).PrimaryKey()
                .WithColumn("questionnaireid").AsGuid()
                .WithColumn("questionnaireversion").AsInt32()
                .WithColumn("entrytype").AsInt32()
                .WithColumn("timestamp").AsDateTime();

            Create.Table("tabletdocuments")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("deviceid").AsGuid()
                .WithColumn("androidid").AsString()
                .WithColumn("registrationdate").AsDateTime();

            Create.Table("timespanbetweenstatuses")
                .WithColumn("id").AsInt32().PrimaryKey()
                .WithColumn("supervisorid").AsGuid()
                .WithColumn("supervisorname").AsString()
                .WithColumn("interviewerid").AsGuid()
                .WithColumn("interviewername").AsString()
                .WithColumn("beginstatus").AsInt32()
                .WithColumn("endstatus").AsInt32()
                .WithColumn("endstatustimestamp").AsDateTime()
                .WithColumn("timespan").AsInt32()
                .WithColumn("interviewstatustimespans").AsString(255)
                .WithColumn("interviewid").AsString(255);

            Create.Table("userchangedfeedentries")
                .WithColumn("entryid").AsString(255).PrimaryKey()
                .WithColumn("changeduserid").AsString()
                .WithColumn("timestamp").AsDateTime()
                .WithColumn("supervisorid").AsString()
                .WithColumn("entrytype").AsInt32();

            Create.Table("lastpublishedeventpositionforhandlers")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("eventsourceidoflastsuccessfullyhandledevent").AsGuid()
                .WithColumn("eventsequenceoflastsuccessfullyhandledevent").AsInt32()
                .WithColumn("commitposition").AsInt32()
                .WithColumn("prepareposition").AsInt32();

            Create.Index("cumulativereportstatuschanges_questionnaire")
                .OnTable("cumulativereportstatuschanges")
                .OnColumn("questionnaireid").Ascending()
                .OnColumn("questionnaireversion").Ascending();

            Create.Index("cumulativereportstatuschanges_date")
                .OnTable("cumulativereportstatuschanges")
                .OnColumn("date");

            Create.Index("interviewcommentaries_comment")
                .OnTable("commentaries")
                .OnColumn("interviewid");

            Create.Index("export_record_interviewid_indx")
                .OnTable("interviewdataexportrecords")
                .OnColumn("interviewid");

            Create.Index("interviewstatuseses_interviewcommentedstatuses")
                .OnTable("interviewcommentedstatuses")
                .OnColumn("interviewid");

            Create.Index("interviewsummaries_responsibleid")
                .OnTable("interviewsummaries")
                .OnColumn("responsibleid");

            Create.Index("interviewsummaries_status")
                .OnTable("interviewsummaries")
                .OnColumn("status");

            Create.Index("interviewsummaries_questionanswers")
                .OnTable("answerstofeaturedquestions")
                .OnColumn("interviewsummaryid");

            
            Create.Index("interviewstatustimespans_timespansbetweenstatuses")
                .OnTable("timespanbetweenstatuses")
                .OnColumn("interviewstatustimespans");

            Create.Index("interviewstatustimespans_interviewid")
                .OnTable("timespanbetweenstatuses")
                .OnColumn("interviewid");

            Create.ForeignKey("fk_interviewsummaries_answerstofeaturedquestions")
                .FromTable("answerstofeaturedquestions")
                .ForeignColumn("interviewsummaryid")
                .ToTable("interviewsummaries").PrimaryColumn("summaryid");

            Create.ForeignKey("interviewcommentedstatuses")
               .FromTable("interviewcommentedstatuses")
               .ForeignColumn("interviewid")
               .ToTable("interviewstatuses").PrimaryColumn("id");

            Create.ForeignKey("fk_commentaries_interviews")
               .FromTable("commentaries").ForeignColumn("interviewid")
               .ToTable("interviewcommentaries").PrimaryColumn("id");

            Create.ForeignKey("fk_interviewstatustimespans_timespanbetweenstatuses")
                .FromTable("timespanbetweenstatuses")
                .ForeignColumn("interviewstatustimespans")
                .ToTable("interviewstatustimespans").PrimaryColumn("id");

            Create.ForeignKey("fk_interviewstatustimespans_timespansbetweenstatuses")
                .FromTable("timespanbetweenstatuses")
                .ForeignColumn("interviewid")
                .ToTable("interviewstatustimespans").PrimaryColumn("id");

            Create.Table("hibernate_unique_key")
                .WithColumn("next_hi").AsInt32();

            Insert.IntoTable("hibernate_unique_key").Row(new {next_hi = 1});
        }

        public override void Down()
        {
            Delete.Table("hibernate_unique_key");
            Delete.Table("lastpublishedeventpositionforhandlers");
            Delete.Table("userchangedfeedentries");
            Delete.Table("timespanbetweenstatuses");
            Delete.Table("tabletdocuments");
            Delete.Table("questionnairefeedentries");
            Delete.Table("interviewsyncpackagemetas");
            Delete.Table("answerstofeaturedquestions");
            Delete.Table("interviewdataexportrecords");
            Delete.Table("interviewsummaries");
            Delete.Table("interviewstatustimespans");
            Delete.Table("interviewcommentedstatuses");
            Delete.Table("interviewstatuses");
            Delete.Table("interviewresponsibles");
            Delete.Table("mapreportpoints");
            Delete.Table("commentaries");
            Delete.Table("interviewfeedentries");
        }
    }
}