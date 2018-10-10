using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WB.Services.Scheduler.Storage
{
    public static class EfCoreHelpers
    {
        public static PropertyBuilder<TEnum> HasConversionOfEnumToString<TEnum>(this PropertyBuilder<TEnum> prop)
        {
            return prop.HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => (TEnum)Enum.Parse(typeof(TEnum), v, true));
        }

        public static void UseSnakeCaseNaming(this ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // Replace table names
                entity.Relational().TableName = entity.Relational().TableName.ToSnakeCase();

                // Replace column names            
                foreach (var property in entity.GetProperties())
                {
                    property.Relational().ColumnName = property.Name.ToSnakeCase();
                }

                foreach (var key in entity.GetKeys())
                {
                    key.Relational().Name = key.Relational().Name.ToSnakeCase();
                }

                foreach (var key in entity.GetForeignKeys())
                {
                    key.Relational().Name = key.Relational().Name.ToSnakeCase();
                }

                foreach (var index in entity.GetIndexes())
                {
                    index.Relational().Name = index.Relational().Name.ToSnakeCase();
                }
            }
        }

        public static Task<bool> TryAcquireLockAsync(this DbContext db, long id)
        {
            return db.Database.GetDbConnection()
                .QuerySingleAsync<bool>($"select pg_try_advisory_lock ({id})");
        }

        public static Task AcquireLockAsync(this DbContext db, long id)
        {
            return db.Database.ExecuteSqlCommandAsync($"select pg_advisory_xact_lock ({id})");
        }

        public static async Task<bool> ReleaseLockAsync(this DbContext db, long id)
        {
            return await db.Database.GetDbConnection()
                .QuerySingleAsync<bool>($"select pg_advisory_unlock ({id})");
        }
    }
}
