using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(2025_12_12_19_28)]
    public class M202512121928_AspNetUsersAddAssistantEnabled : ForwardOnlyMigration
    {
        public override void Up()
        {
            Alter.Table("AspNetUsers")
            .AddColumn("AssistantEnabled").AsBoolean().Nullable();
        }
    }
}
