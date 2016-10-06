using FluentMigrator.VersionTableInfo;

namespace WB.Infrastructure.Native.Storage.Postgre.DbMigrations
{
    [VersionTableMetaData]
    public class InSchemaVersionTable : DefaultVersionTableMetaData
    {
        private string schemaName;

        public override string SchemaName => this.schemaName;

        public void SetSchemaName(string value)
        {
            this.schemaName = value;
        }
    }
}