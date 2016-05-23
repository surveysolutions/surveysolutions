using System.Web.WebPages;
using FluentMigrator;

namespace WB.UI.Designer.Migrations
{
    [Migration(1)]
    public class M001_Init : Migration
    {
        public override void Up()
        {
            Create.Table("accountdocuments")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("applicationname").AsString()
                .WithColumn("comment").AsString()
                .WithColumn("confirmationtoken").AsString()
                .WithColumn("createdat").AsDateTime()
                .WithColumn("email").AsString()
                .WithColumn("isconfirmed").AsBoolean()
                .WithColumn("islockedout").AsBoolean()
                .WithColumn("isonline").AsBoolean()
                .WithColumn("lastactivityat").AsDateTime()
                .WithColumn("lastlockedoutat").AsDateTime()
                .WithColumn("lastloginat").AsDateTime()
                .WithColumn("lastpasswordchangeat").AsDateTime()
                .WithColumn("password").AsString()
                .WithColumn("passwordanswer").AsString()
                .WithColumn("passwordquestion").AsString()
                .WithColumn("passwordresetexpirationdate").AsDateTime()
                .WithColumn("passwordresettoken").AsString()
                .WithColumn("passwordsalt").AsString()
                .WithColumn("provideruserkey").AsGuid()
                .WithColumn("username").AsString();

            Create.Table("simpleroles")
                .WithColumn("accountid").AsString(255).NotNullable()
                .WithColumn("simpleroleid").AsInt32();

            Create.Table("questionnairechangerecords")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("questionnaireid").AsString()
                .WithColumn("userid").AsGuid()
                .WithColumn("username").AsString()
                .WithColumn("timestamp").AsDateTime()
                .WithColumn("sequence").AsInt32()
                .WithColumn("actiontype").AsInt32()
                .WithColumn("targetitemtype").AsInt32()
                .WithColumn("targetitemid").AsGuid()
                .WithColumn("targetitemtitle").AsString();

            Create.Table("questionnairelistviewitems")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("creationdate").AsDateTime()
                .WithColumn("publicid").AsGuid()
                .WithColumn("lastentrydate").AsDateTime()
                .WithColumn("title").AsString()
                .WithColumn("createdby").AsGuid()
                .WithColumn("creatorname").AsString()
                .WithColumn("isdeleted").AsBoolean()
                .WithColumn("ispublic").AsBoolean()
                .WithColumn("owner").AsString();

            Create.Table("sharedpersons")
                .WithColumn("questionnaireid").AsString(255).NotNullable()
                .WithColumn("sharedpersonid").AsGuid();

            Create.Table("questionnairechangereferences")
                .WithColumn("id").AsInt32().PrimaryKey()
                .WithColumn("referencetype").AsInt32()
                .WithColumn("referenceid").AsGuid()
                .WithColumn("referencetitle").AsString()
                .WithColumn("questionnairechangerecord").AsString(255)
                .WithColumn("questionnairechangerecordid").AsString(255);

            Create.ForeignKey()
                .FromTable("simpleroles").ForeignColumn("accountid")
                .ToTable("accountdocuments").PrimaryColumn("id");

            Create.Index("questionnairechangerecord_userid")
                .OnTable("questionnairechangerecords").OnColumn("userid");

            Create.Index("questionnairechangerecord_username")
                .OnTable("questionnairechangerecords").OnColumn("questionnairechangerecord_username");

            Create.Index("questionnairelistviewitem_sharedpersons")
                .OnTable("sharedpersons").OnColumn("questionnaireid");

            Create.ForeignKey()
                .FromTable("sharedpersons").ForeignColumn("questionnaireid")
                .ToTable("questionnairelistviewitems").PrimaryColumn("id");

            Create.Index("questionnairechangerecords_questionnairechangereferences")
                .OnTable("questionnairechangereferences").OnColumn("questionnairechangerecord");

            Create.ForeignKey()
                .FromTable("questionnairechangereferences").ForeignColumn("questionnairechangerecord")
                .ToTable("questionnairechangerecords").PrimaryColumn("id");

            Create.ForeignKey()
                .FromTable("questionnairechangereferences").ForeignColumn("questionnairechangerecordid")
                .ToTable("questionnairechangerecords").PrimaryColumn("id");
        }

        public override void Down()
        {
            Delete.Table("AccountDocuments");
            Delete.Table("SimpleRoles");
            Delete.Table("SimpleRoles");
            Delete.Table("QuestionnaireChangeRecords");
            Delete.Table("QuestionnaireListViewItems");
            Delete.Table("SharedPersons");
            Delete.Table("QuestionnaireChangeReferences");
            Delete.Table("hibernate_unique_key");
        }
    }
}