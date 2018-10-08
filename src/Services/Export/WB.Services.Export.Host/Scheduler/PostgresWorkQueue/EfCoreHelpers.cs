using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WB.Services.Export.Host.Scheduler.PostgresWorkQueue
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
    }
}
