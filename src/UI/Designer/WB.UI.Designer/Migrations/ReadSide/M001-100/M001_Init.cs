using FluentMigrator;

namespace WB.UI.Designer.Migrations.ReadSide
{
    [Migration(1)]
    public class M001_Init : Migration
    {
        public override void Up()
        {
            this.Create.Table("accountdocuments")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("applicationname").AsString().Nullable()
                .WithColumn("comment").AsString().Nullable()
                .WithColumn("confirmationtoken").AsString().Nullable()
                .WithColumn("createdat").AsDateTime().Nullable()
                .WithColumn("email").AsString().Nullable()
                .WithColumn("isconfirmed").AsBoolean().Nullable()
                .WithColumn("islockedout").AsBoolean().Nullable()
                .WithColumn("isonline").AsBoolean().Nullable()
                .WithColumn("lastactivityat").AsDateTime().Nullable()
                .WithColumn("lastlockedoutat").AsDateTime().Nullable()
                .WithColumn("lastloginat").AsDateTime().Nullable()
                .WithColumn("lastpasswordchangeat").AsDateTime().Nullable()
                .WithColumn("password").AsString().Nullable()
                .WithColumn("passwordanswer").AsString().Nullable()
                .WithColumn("passwordquestion").AsString().Nullable()
                .WithColumn("passwordresetexpirationdate").AsDateTime().Nullable()
                .WithColumn("passwordresettoken").AsString().Nullable()
                .WithColumn("passwordsalt").AsString().Nullable()
                .WithColumn("provideruserkey").AsGuid().Nullable()
                .WithColumn("username").AsString().Nullable();

            this.Create.Table("simpleroles")
                .WithColumn("accountid").AsString(255)
                .WithColumn("simpleroleid").AsInt32().Nullable();

            this.Create.Table("questionnairechangerecords")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("questionnaireid").AsString().Nullable()
                .WithColumn("userid").AsGuid().Nullable()
                .WithColumn("username").AsString().Nullable()
                .WithColumn("timestamp").AsDateTime().Nullable()
                .WithColumn("sequence").AsInt32().Nullable()
                .WithColumn("actiontype").AsInt32().Nullable()
                .WithColumn("targetitemtype").AsInt32().Nullable()
                .WithColumn("targetitemid").AsGuid().Nullable()
                .WithColumn("targetitemtitle").AsString().Nullable();

            this.Create.Table("questionnairelistviewitems")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("creationdate").AsDateTime().Nullable()
                .WithColumn("publicid").AsGuid().Nullable()
                .WithColumn("lastentrydate").AsDateTime().Nullable()
                .WithColumn("title").AsString().Nullable()
                .WithColumn("createdby").AsGuid().Nullable()
                .WithColumn("creatorname").AsString().Nullable()
                .WithColumn("isdeleted").AsBoolean().Nullable()
                .WithColumn("ispublic").AsBoolean().Nullable()
                .WithColumn("owner").AsString().Nullable();

            this.Create.Table("sharedpersons")
                .WithColumn("questionnaireid").AsString(255)
                .WithColumn("sharedpersonid").AsGuid().Nullable();

            this.Create.Table("questionnairechangereferences")
                .WithColumn("id").AsInt32().PrimaryKey()
                .WithColumn("referencetype").AsInt32().Nullable()
                .WithColumn("referenceid").AsGuid().Nullable()
                .WithColumn("referencetitle").AsString().Nullable()
                .WithColumn("questionnairechangerecord").AsString(255).Nullable()
                .WithColumn("questionnairechangerecordid").AsString(255).Nullable();

            this.Create.ForeignKey()
                .FromTable("simpleroles").ForeignColumn("accountid")
                .ToTable("accountdocuments").PrimaryColumn("id");

            this.Create.Index("questionnairechangerecord_userid")
                .OnTable("questionnairechangerecords").OnColumn("userid");

            this.Create.Index("questionnairechangerecord_username")
                .OnTable("questionnairechangerecords").OnColumn("username");

            this.Create.Index("questionnairelistviewitem_sharedpersons")
                .OnTable("sharedpersons").OnColumn("questionnaireid");

            this.Create.ForeignKey()
                .FromTable("sharedpersons").ForeignColumn("questionnaireid")
                .ToTable("questionnairelistviewitems").PrimaryColumn("id");

            this.Create.Index("questionnairechangerecords_questionnairechangereferences")
                .OnTable("questionnairechangereferences").OnColumn("questionnairechangerecord");

            this.Create.ForeignKey()
                .FromTable("questionnairechangereferences").ForeignColumn("questionnairechangerecord")
                .ToTable("questionnairechangerecords").PrimaryColumn("id");

            this.Create.ForeignKey()
                .FromTable("questionnairechangereferences").ForeignColumn("questionnairechangerecordid")
                .ToTable("questionnairechangerecords").PrimaryColumn("id");

            Create.Table("hibernate_unique_key")
              .WithColumn("next_hi").AsInt32();

            Insert.IntoTable("hibernate_unique_key").Row(new { next_hi = 1 });
        }

        public override void Down()
        {
            this.Delete.Table("AccountDocuments");
            this.Delete.Table("SimpleRoles");
            this.Delete.Table("SimpleRoles");
            this.Delete.Table("QuestionnaireChangeRecords");
            this.Delete.Table("QuestionnaireListViewItems");
            this.Delete.Table("SharedPersons");
            this.Delete.Table("QuestionnaireChangeReferences");
            this.Delete.Table("hibernate_unique_key");
        }
    }
}