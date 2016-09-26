using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(6)]
    public class M006_AddTablesNeededForStoreQuestionnaireInPlainStorage : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("questionnairechangerecords").Exists())
            {
                Create.Table("questionnairechangerecords")
                    .WithColumn("id").AsString(255).PrimaryKey("questionnairechangerecords_pkey")
                    .WithColumn("questionnaireid").AsString().Nullable()
                    .WithColumn("userid").AsGuid().Nullable()
                    .WithColumn("username").AsString().Nullable()
                    .WithColumn("timestamp").AsDateTime().Nullable()
                    .WithColumn("sequence").AsInt32().Nullable()
                    .WithColumn("actiontype").AsInt32().Nullable()
                    .WithColumn("targetitemtype").AsInt32().Nullable()
                    .WithColumn("targetitemid").AsGuid().Nullable()
                    .WithColumn("targetitemtitle").AsString().Nullable();

                Create.Index("questionnairechangerecord_userid")
                    .OnTable("questionnairechangerecords")
                    .OnColumn("userid");

                Create.Index("questionnairechangerecord_username")
                    .OnTable("questionnairechangerecords")
                    .OnColumn("username");
            }

            if (!Schema.Table("questionnairechangereferences").Exists())
            {
                Create.Table("questionnairechangereferences")
                    .WithColumn("id").AsInt32().PrimaryKey("questionnairechangereferences_pkey")
                    .WithColumn("referencetype").AsInt32().Nullable()
                    .WithColumn("referenceid").AsGuid().Nullable()
                    .WithColumn("referencetitle").AsString().Nullable()
                    .WithColumn("questionnairechangerecord").AsString(255).Nullable()
                    .WithColumn("questionnairechangerecordid").AsString(255).Nullable();

                Create.Index("questionnairechangerecords_questionnairechangereferences")
                    .OnTable("questionnairechangereferences")
                    .OnColumn("questionnairechangerecord");

                Create.ForeignKey("questionnairechangereferences_questionnairechangerecordid")
                    .FromTable("questionnairechangereferences").ForeignColumn("questionnairechangerecordid")
                    .ToTable("questionnairechangerecords").PrimaryColumn("id");

                Create.ForeignKey("questionnairechangereferences_questionnairechangerecord")
                    .FromTable("questionnairechangereferences").ForeignColumn("questionnairechangerecord")
                    .ToTable("questionnairechangerecords").PrimaryColumn("id");
            }

            if (!Schema.Table("questionnairelistviewitems").Exists())
            {
                Create.Table("questionnairelistviewitems")
                    .WithColumn("id").AsString(255).PrimaryKey("questionnairelistviewitems_pkey")
                    .WithColumn("creationdate").AsDateTime().Nullable()
                    .WithColumn("publicid").AsGuid().Nullable()
                    .WithColumn("lastentrydate").AsDateTime().Nullable()
                    .WithColumn("title").AsString().Nullable()
                    .WithColumn("createdby").AsGuid().Nullable()
                    .WithColumn("creatorname").AsString().Nullable()
                    .WithColumn("isdeleted").AsBoolean().Nullable()
                    .WithColumn("ispublic").AsBoolean().Nullable()
                    .WithColumn("owner").AsString().Nullable();
            }

            if (!Schema.Table("sharedpersons").Exists())
            {
                Create.Table("sharedpersons")
                    .WithColumn("questionnaireid").AsString(255)
                    .WithColumn("sharedpersonid").AsGuid().Nullable();

                Create.Index("questionnairelistviewitem_sharedpersons")
                    .OnTable("sharedpersons")
                    .OnColumn("questionnaireid");

                Create.ForeignKey("questionnairechangerecord_questionnaireid")
                    .FromTable("sharedpersons").ForeignColumn("questionnaireid")
                    .ToTable("questionnairelistviewitems").PrimaryColumn("id");
            }
        }

        public override void Down()
        {
            DropTableIfExists("sharedpersons");
            DropTableIfExists("questionnairelistviewitems");
            DropTableIfExists("questionnairechangereferences");
            DropTableIfExists("questionnairechangerecords");
        }

        private void DropTableIfExists(string tableName)
        {
            if (Schema.Table(tableName).Exists())
            {
                Delete.Table(tableName);
            }
        }
    }
}