using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(1)]
    public class M001_InitDb : Migration
    {
        public override void Up()
        {
            Create.Table("cumulativereportstatuschanges")
                .WithColumn("entryid").AsString(255).NotNullable().PrimaryKey()
                .WithColumn("questionnaireid").AsGuid().Nullable()
                .WithColumn("questionnaireversion").AsInt64().Nullable()
                .WithColumn("date").AsDateTime().Nullable()
                .WithColumn("status").AsInt32().Nullable()
                .WithColumn("changevalue").AsInt32().Nullable();

            Create.Table("interviewcommentaries")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("isdeleted").AsBoolean().Nullable()
                .WithColumn("isapprovedbyhq").AsBoolean().Nullable()
                .WithColumn("questionnaireid").AsString().Nullable()
                .WithColumn("questionnaireversion").AsInt64().Nullable();

            Create.Table("commentaries")
                .WithColumn("interviewid").AsString(255).PrimaryKey()
                .WithColumn("position").AsInt32().PrimaryKey()
                .WithColumn("commentsequence").AsInt32().Nullable()
                .WithColumn("originatorname").AsString().Nullable()
                .WithColumn("originatoruserid").AsGuid().Nullable()
                .WithColumn("originatorrole").AsInt32().Nullable()
                .WithColumn("timestamp").AsDateTime().Nullable()
                .WithColumn("variable").AsString().Nullable()
                .WithColumn("roster").AsString().Nullable()
                .WithColumn("rostervector").AsCustom("numeric[]").Nullable()
                .WithColumn("comment").AsString().Nullable();

            Create.Table("interviewdataexportrecords")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("recordid").AsString().Nullable()
                .WithColumn("interviewid").AsGuid().Nullable()
                .WithColumn("levelname").AsString().Nullable()
                .WithColumn("parentrecordids").AsCustom("text[]").Nullable()
                .WithColumn("referencevalues").AsCustom("text[]").Nullable()
                .WithColumn("systemvariablevalues").AsCustom("text[]").Nullable()
                .WithColumn("answers").AsCustom("text[]").Nullable();

            Create.Table("mapreportpoints")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("questionnaireid").AsGuid().Nullable()
                .WithColumn("questionnaireversion").AsInt64().Nullable()
                .WithColumn("variable").AsString().Nullable()
                .WithColumn("interviewid").AsGuid().Nullable()
                .WithColumn("latitude").AsDouble().Nullable()
                .WithColumn("longitude").AsDouble().Nullable();

            Create.Table("interviewresponsibles")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("interviewid").AsGuid().Nullable()
                .WithColumn("userid").AsGuid().Nullable();

            Create.Table("interviewstatuses")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("questionnaireid").AsGuid().Nullable()
                .WithColumn("questionnaireversion").AsInt64().Nullable();

            Create.Table("interviewcommentedstatuses")
                .WithColumn("interviewid").AsString(255).PrimaryKey()
                .WithColumn("position").AsInt32().PrimaryKey()
                .WithColumn("id").AsGuid().Nullable()
                .WithColumn("interviewername").AsString().Nullable()
                .WithColumn("interviewerid").AsGuid().Nullable()
                .WithColumn("supervisorid").AsGuid().Nullable()
                .WithColumn("supervisorname").AsString().Nullable()
                .WithColumn("statuschangeoriginatorid").AsGuid().Nullable()
                .WithColumn("statuschangeoriginatorname").AsString().Nullable()
                .WithColumn("statuschangeoriginatorrole").AsInt32().Nullable()
                .WithColumn("status").AsInt32().Nullable()
                .WithColumn("timestamp").AsDateTime().Nullable()
                .WithColumn("timespanwithpreviousstatus").AsInt64().Nullable()
                .WithColumn("comment").AsString().Nullable();

            Create.Table("interviewstatustimespans")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("questionnaireid").AsGuid().Nullable()
                .WithColumn("questionnaireversion").AsInt64().Nullable();

            Create.Table("interviewsummaries")
                .WithColumn("summaryid").AsString(255).PrimaryKey()
                .WithColumn("interviewid").AsGuid().Nullable()
                .WithColumn("questionnairetitle").AsString().Nullable()
                .WithColumn("responsiblename").AsString().Nullable()
                .WithColumn("teamleadid").AsGuid().Nullable()
                .WithColumn("teamleadname").AsString().Nullable()
                .WithColumn("responsiblerole").AsInt32().Nullable()
                .WithColumn("updatedate").AsDateTime().Nullable()
                .WithColumn("wasrejectedbysupervisor").AsBoolean().Nullable()
                .WithColumn("wascreatedonclient").AsBoolean().Nullable()
                .WithColumn("receivedbyinterviewer").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("questionnaireid").AsGuid().Nullable()
                .WithColumn("questionnaireversion").AsInt64().Nullable()
                .WithColumn("responsibleid").AsGuid().Nullable()
                .WithColumn("status").AsInt32().Nullable()
                .WithColumn("isdeleted").AsBoolean().Nullable()
                .WithColumn("haserrors").AsBoolean().Nullable();

            Create.Table("answerstofeaturedquestions")
                .WithColumn("id").AsInt32().PrimaryKey()
                .WithColumn("questionid").AsGuid().Nullable()
                .WithColumn("answertitle").AsString().Nullable()
                .WithColumn("answervalue").AsString().Nullable()
                .WithColumn("interviewsummaryid").AsString(255).Nullable();

            Create.Table("interviewsyncpackagemetas")
                .WithColumn("packageid").AsString(255).PrimaryKey()
                .WithColumn("sortindex").AsInt64().Nullable()
                .WithColumn("userid").AsGuid().Nullable()
                .WithColumn("interviewid").AsGuid().Nullable()
                .WithColumn("itemtype").AsString().Nullable()
                .WithColumn("serializedpackagesize").AsInt32().Nullable();

           
            Create.Table("tabletdocuments")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("deviceid").AsGuid().Nullable()
                .WithColumn("androidid").AsString().Nullable()
                .WithColumn("registrationdate").AsDateTime().Nullable();

            Create.Table("timespanbetweenstatuses")
                .WithColumn("id").AsInt32().PrimaryKey()
                .WithColumn("supervisorid").AsGuid().Nullable()
                .WithColumn("supervisorname").AsString().Nullable()
                .WithColumn("interviewerid").AsGuid().Nullable()
                .WithColumn("interviewername").AsString().Nullable()
                .WithColumn("beginstatus").AsInt32().Nullable()
                .WithColumn("endstatus").AsInt32().Nullable()
                .WithColumn("endstatustimestamp").AsDateTime().Nullable()
                .WithColumn("timespan").AsInt64().Nullable()
                .WithColumn("interviewstatustimespans").AsString(255).Nullable()
                .WithColumn("interviewid").AsString(255).Nullable();

            Create.Table("lastpublishedeventpositionforhandlers")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("eventsourceidoflastsuccessfullyhandledevent").AsGuid().Nullable()
                .WithColumn("eventsequenceoflastsuccessfullyhandledevent").AsInt32().Nullable()
                .WithColumn("commitposition").AsInt64().Nullable()
                .WithColumn("prepareposition").AsInt64().Nullable();

            /* to be removed ? */
            Create.Table("interviewfeedentries")
                .WithColumn("entryid").AsString(255).PrimaryKey()
                .WithColumn("supervisorid").AsString().Nullable()
                .WithColumn("entrytype").AsInt32().Nullable()
                .WithColumn("timestamp").AsDateTime().Nullable()
                .WithColumn("interviewid").AsString().Nullable()
                .WithColumn("userid").AsString().Nullable()
                .WithColumn("interviewerid").AsString().Nullable();

            Create.Table("questionnairefeedentries")
               .WithColumn("entryid").AsString(255).PrimaryKey()
               .WithColumn("questionnaireid").AsGuid().Nullable()
               .WithColumn("questionnaireversion").AsInt64().Nullable()
               .WithColumn("entrytype").AsInt32().Nullable()
               .WithColumn("timestamp").AsDateTime().Nullable();

            Create.Table("userchangedfeedentries")
                .WithColumn("entryid").AsString(255).PrimaryKey()
                .WithColumn("changeduserid").AsString().Nullable()
                .WithColumn("timestamp").AsDateTime().Nullable()
                .WithColumn("supervisorid").AsString().Nullable()
                .WithColumn("entrytype").AsInt32().Nullable();

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
            Delete.Table("timespanbetweenstatuses");
            Delete.Table("tabletdocuments");
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

            Delete.Table("questionnairefeedentries");
            Delete.Table("userchangedfeedentries");
            Delete.Table("interviewfeedentries");
        }
    }
}