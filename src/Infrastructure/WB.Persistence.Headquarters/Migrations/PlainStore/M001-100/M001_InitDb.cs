using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(1)]
    public class M001_Init : Migration
    {
        public override void Up()
        {
            Create.Table("brokeninterviewpackages")
              .WithColumn("id").AsInt32().Identity().PrimaryKey()
              .WithColumn("interviewid").AsGuid().Nullable()
              .WithColumn("questionnaireid").AsGuid().Nullable()
              .WithColumn("questionnaireversion").AsInt64().Nullable()
              .WithColumn("responsibleid").AsGuid().Nullable()
              .WithColumn("interviewstatus").AsInt32().Nullable()
              .WithColumn("iscensusinterview").AsBoolean().Nullable()
              .WithColumn("incomingdate").AsDateTime().Nullable()
              .WithColumn("events").AsString().Nullable()
              .WithColumn("processingdate").AsDateTime().Nullable()
              .WithColumn("exceptiontype").AsString().Nullable()
              .WithColumn("exceptionmessage").AsString().Nullable()
              .WithColumn("exceptionstacktrace").AsString().Nullable()
              .WithColumn("packagesize").AsInt64().Nullable();

            Create.Table("attachmentcontents")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("contenttype").AsString().Nullable()
                .WithColumn("content").AsBinary().Nullable();

            Create.Table("synchronizationlog")
                .WithColumn("id").AsInt32().PrimaryKey()
                .WithColumn("interviewerid").AsGuid().Nullable()
                .WithColumn("interviewername").AsString().Nullable()
                .WithColumn("deviceid").AsString().Nullable()
                .WithColumn("logdate").AsDateTime().Nullable()
                .WithColumn("type").AsInt32().Nullable()
                .WithColumn("log").AsString().Nullable();

            Create.Table("interviewpackages")
                .WithColumn("id").AsInt32().Identity().PrimaryKey()
                .WithColumn("interviewid").AsGuid().Nullable()
                .WithColumn("questionnaireid").AsGuid().Nullable()
                .WithColumn("questionnaireversion").AsInt64().Nullable()
                .WithColumn("responsibleid").AsGuid().Nullable()
                .WithColumn("interviewstatus").AsInt32().Nullable()
                .WithColumn("iscensusinterview").AsBoolean().Nullable()
                .WithColumn("incomingdate").AsDateTime().Nullable()
                .WithColumn("events").AsString().Nullable();

            Create.Table("questionnairebrowseitems")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("creationdate").AsDateTime().Nullable()
                .WithColumn("questionnaireid").AsGuid().Nullable()
                .WithColumn("version").AsInt64().Nullable()
                .WithColumn("lastentrydate").AsDateTime().Nullable()
                .WithColumn("title").AsString().Nullable()
                .WithColumn("ispublic").AsBoolean().Nullable()
                .WithColumn("createdby").AsGuid().Nullable()
                .WithColumn("isdeleted").AsBoolean().Nullable()
                .WithColumn("allowcensusmode").AsBoolean().Nullable()
                .WithColumn("disabled").AsBoolean().Nullable()
                .WithColumn("questionnairecontentversion").AsInt64().Nullable();

            Create.Table("featuredquestions")
                .WithColumn("questionnaireid").AsString(255).PrimaryKey()
                .WithColumn("position").AsInt32().PrimaryKey()
                .WithColumn("id").AsGuid().Nullable()
                .WithColumn("title").AsString().Nullable()
                .WithColumn("caption").AsString().Nullable();


            Create.Table("userdocuments")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("creationdate").AsDateTime().Nullable()
                .WithColumn("email").AsString().Nullable()
                .WithColumn("islockedbyhq").AsBoolean().Nullable()
                .WithColumn("isarchived").AsBoolean().Nullable()
                .WithColumn("islockedbysupervisor").AsBoolean().Nullable()
                .WithColumn("password").AsString().Nullable()
                .WithColumn("publickey").AsGuid().Nullable()
                .WithColumn("supervisorid").AsGuid().Nullable()
                .WithColumn("supervisorname").AsString().Nullable()
                .WithColumn("username").AsString().Nullable()
                .WithColumn("lastchangedate").AsDateTime().Nullable()
                .WithColumn("deviceid").AsString().Nullable()
                .WithColumn("personname").AsString().Nullable()
                .WithColumn("phonenumber").AsString().Nullable();

            Create.Table("roles")
                .WithColumn("userid").AsString(255).NotNullable()
                .WithColumn("roleid").AsInt32().Nullable();

            Create.Table("deviceinfos")
                .WithColumn("id").AsInt32().PrimaryKey()
                .WithColumn("User").AsString(255).Nullable()
                .WithColumn("date").AsDateTime().Nullable()
                .WithColumn("deviceid").AsString().Nullable()
                .WithColumn("userid").AsString(255).Nullable();

            Create.Table("userpreloadingprocesses")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("filename").AsString().Nullable()
                .WithColumn("state").AsInt32().Nullable()
                .WithColumn("filesize").AsInt64().Nullable()
                .WithColumn("uploaddate").AsDateTime().Nullable()
                .WithColumn("validationstartdate").AsDateTime().Nullable()
                .WithColumn("verificationprogressinpercents").AsInt32().Nullable()
                .WithColumn("creationstartdate").AsDateTime().Nullable()
                .WithColumn("lastupdatedate").AsDateTime().Nullable()
                .WithColumn("recordscount").AsInt64().Nullable()
                .WithColumn("createduserscount").AsInt64().Nullable()
                .WithColumn("errormessage").AsString().Nullable();

            Create.Table("userprelodingdata")
                .WithColumn("userpreloadingprocessid").AsString(255).PrimaryKey()
                .WithColumn("position").AsInt32().PrimaryKey()
                .WithColumn("login").AsString().Nullable()
                .WithColumn("password").AsString().Nullable()
                .WithColumn("email").AsString().Nullable()
                .WithColumn("fullname").AsString().Nullable()
                .WithColumn("phonenumber").AsString().Nullable()
                .WithColumn("role").AsString().Nullable()
                .WithColumn("supervisor").AsString().Nullable();

            Create.Table("userpreloadingverificationerrors")
                .WithColumn("id").AsInt32().PrimaryKey()
                .WithColumn("userpreloadingprocess").AsString(255).Nullable()
                .WithColumn("code").AsString().Nullable()
                .WithColumn("rownumber").AsInt32().Nullable()
                .WithColumn("columnname").AsString().Nullable()
                .WithColumn("cellvalue").AsString().Nullable()
                .WithColumn("userpreloadingprocessid").AsString(255).Nullable();

            Create.Table("productversionhistory")
                .WithColumn("updatetimeutc").AsDateTime().PrimaryKey()
                .WithColumn("productversion").AsString().Nullable();

            Create.Index("interviewpackage_interviewid").OnTable("interviewpackages").OnColumn("interviewid");
            Create.Index("questionnairebrowseitems_featuredquestions").OnTable("featuredquestions").OnColumn("questionnaireid");

            Create.Index("user_creationdate").OnTable("userdocuments").OnColumn("creationdate");
            Create.Index("user_email").OnTable("userdocuments").OnColumn("email");
            Create.Index("user_publickey").OnTable("userdocuments").OnColumn("publickey");
            Create.Index("user_supervisorid").OnTable("userdocuments").OnColumn("supervisorid");
            Create.Index("user_deviceid").OnTable("userdocuments").OnColumn("deviceid");
            Create.Index("user_personname").OnTable("userdocuments").OnColumn("personname");
            Create.Index("users_roles_fk").OnTable("roles").OnColumn("userid");
            Create.Index("userdocuments_deviceinfos").OnTable("deviceinfos").OnColumn("User");

            Create.Index("userpreloadingverificationerrors_userpreloadingprocesses")
                .OnTable("userpreloadingverificationerrors")
                .OnColumn("userpreloadingprocess");

            Create.Index("userpreloadingdatarecords_userpreloadingprocesses")
                .OnTable("userprelodingdata")
                .OnColumn("userpreloadingprocessid");

            Create.ForeignKey("fk_questionnairebrowseitems_featuredquestions")
                .FromTable("featuredquestions").ForeignColumn("questionnaireid")
                .ToTable("questionnairebrowseitems").PrimaryColumn("id");

            Create.ForeignKey()
                .FromTable("roles").ForeignColumn("userid")
                .ToTable("userdocuments").PrimaryColumn("id");

            Create.ForeignKey()
                .FromTable("deviceinfos").ForeignColumn("User")
                .ToTable("userdocuments").PrimaryColumn("id");

            Create.ForeignKey()
                .FromTable("deviceinfos").ForeignColumn("userid")
                .ToTable("userdocuments").PrimaryColumn("id");

            Create.ForeignKey()
                .FromTable("userprelodingdata").ForeignColumn("userpreloadingprocessid")
                .ToTable("userpreloadingprocesses").PrimaryColumn("id");

            Create.ForeignKey()
                .FromTable("userpreloadingverificationerrors").ForeignColumn("userpreloadingprocess")
                .ToTable("userpreloadingprocesses").PrimaryColumn("id");

            Create.ForeignKey("fk_userpreloadingprocesses_userpreloadingprocessid")
                .FromTable("userpreloadingverificationerrors").ForeignColumn("userpreloadingprocessid")
                .ToTable("userpreloadingprocesses").PrimaryColumn("id");

            Create.Table("hibernate_unique_key")
              .WithColumn("next_hi").AsInt32();

            Insert.IntoTable("hibernate_unique_key").Row(new { next_hi = 1 });
        }

        public override void Down()
        {
            Delete.Table("brokeninterviewpackages");
            Delete.Table("attachmentcontents");
            Delete.Table("synchronizationlog");
            Delete.Table("interviewpackages");
            Delete.Table("questionnairebrowseitems");
            Delete.Table("userdocuments");
            Delete.Table("roles");
            Delete.Table("deviceinfos");
            Delete.Table("userpreloadingprocesses");
            Delete.Table("userprelodingdata");
            Delete.Table("userpreloadingverificationerrors");
            Delete.Table("productversionhistory");
        }
    }
}
