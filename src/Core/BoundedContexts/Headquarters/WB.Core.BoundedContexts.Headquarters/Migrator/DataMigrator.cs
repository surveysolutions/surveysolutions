using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace WB.Core.BoundedContexts.Headquarters.Migrator
{
    /// <summary>
    /// Handmade migration utility for EF. Handle data migrations on Seed execution
    /// </summary>
    /// <typeparam name="TDbContext"></typeparam>
    public class DataMigrator<TDbContext> where TDbContext : DbContext, IDataMigrationContext
    {
        public void MigrateToLatest(TDbContext context)
        {
            var migrationType = typeof(DataMigration<TDbContext>);

            // looking for all migrations in assembly
            var migrations = Assembly.GetCallingAssembly()
                .DefinedTypes
                .Where(dt => migrationType.IsAssignableFrom(dt))
                .Select(dt => Activator.CreateInstance(dt) as DataMigration<TDbContext>)
                .OrderBy(m => m.Id);

            // get list of executed migrations
            var appliedMigrations = new HashSet<string>(context.DataMigrations.OrderBy(m => m.Id).Select(m => m.Id));

            using (var dbContextTransaction = context.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                foreach (var migration in migrations)
                {
                    if (appliedMigrations.Contains(migration.Id)) continue;

                    migration.Up(context);

                    context.DataMigrations.Add(new DataMigrationInfo
                    {
                        Id = migration.Id,
                        Name = migration.GetType().Name
                    });

                    context.SaveChanges();
                }

                dbContextTransaction.Commit();
            }
        }
    }
}