﻿using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(201904041737)]
    public class M201904041737_AddPasswordSalt : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Column("PasswordSalt").OnTable("AspNetUsers").AsString().Nullable();
            Create.Column("CanImportOnHq").OnTable("AspNetUsers").AsBoolean().WithDefaultValue(false).NotNullable();
        }
    }
}
