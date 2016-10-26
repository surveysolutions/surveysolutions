using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Migrations.History;

namespace WB.UI.Headquarters.Migrations
{
    public class SchemaBasedHistoryContext : HistoryContext
    {
        private readonly string schemaName;
        public SchemaBasedHistoryContext(DbConnection dbConnection, string defaultSchema, string schemaName)
            : base(dbConnection, defaultSchema)
        {
            this.schemaName = schemaName;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<HistoryRow>().ToTable(tableName: "migrations", schemaName: this.schemaName);
        }
    }
}