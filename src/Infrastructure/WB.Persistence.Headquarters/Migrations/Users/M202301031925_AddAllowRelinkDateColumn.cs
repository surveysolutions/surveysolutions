using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Users;

[Localizable(false)]
[Migration(202301031925)]
public class M202301031925_AddAllowRelinkDateColumn : AutoReversingMigration
{
    public override void Up()
    {
        Create.Column("AllowRelinkDate")
            .OnTable("userprofiles")
            .AsDateTime()
            .Nullable();
    }
}
