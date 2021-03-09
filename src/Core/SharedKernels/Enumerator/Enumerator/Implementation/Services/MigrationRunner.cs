using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    internal class MigrationRunner : IMigrationRunner
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IPlainStorage<Migration, long> migrationsRepository;
        private readonly ILogger logger;

        public MigrationRunner(IServiceProvider serviceProvider, IPlainStorage<Migration, long> migrationsRepository, ILogger logger)
        {
            this.serviceProvider = serviceProvider;
            this.migrationsRepository = migrationsRepository;
            this.logger = logger;
        }

        public void MigrateUp(Assembly[] scanInAssembly)
        {
            var migrationInfos = scanInAssembly.SelectMany(this.LoadMigrations)
                .Where(x => this.migrationsRepository.Count(y => y.Id == x.Key) == 0)
                .Select(x => x.Value)
                .ToArray();

            this.logger.Trace($"Migrations. {migrationInfos.Length} new migration(s) found");

            foreach (var migrationInfo in migrationInfos)
            {
                var migration = migrationInfo.Migration;
                var migrationDescription = migrationInfo.Description ?? migration.GetType().Name;

                this.logger.Debug($"Migrations. Migration: {migrationDescription}({migrationInfo.Version}) started");

                Stopwatch sw = Stopwatch.StartNew();

                migration.Up();

                this.migrationsRepository.Store(new Migration
                {
                    Id = migrationInfo.Version,
                    Description = migrationDescription
                });

                this.logger.Debug($"Migrations. Migration: {migrationDescription}({migrationInfo.Version}) completed. Took {sw.Elapsed}");
            }
        }

        public SortedList<long, IMigrationInfo> LoadMigrations(Assembly scanInAssembly)
        {
            var sortedMigrations = new SortedList<long, IMigrationInfo>();

            var migrations = scanInAssembly.GetExportedTypes()
                .Where(type =>
                    !type.IsAbstract && typeof(IMigration).IsAssignableFrom(type) &&
                    type.GetCustomAttributes<MigrationAttribute>().Any())
                .Select(this.GetMigrationInfoForMigration)
                .ToList();

            foreach (var migrationInfo in migrations)
            {
                if (sortedMigrations.ContainsKey(migrationInfo.Version))
                    throw new Exception($"Duplicate migration version {migrationInfo.Version}.");

                sortedMigrations.Add(migrationInfo.Version, migrationInfo);
            }

            return sortedMigrations;
        }

        private IMigrationInfo GetMigrationInfoForMigration(Type migrationType)
        {
            var migrationAttribute = migrationType.GetCustomAttribute<MigrationAttribute>();

            return new MigrationInfo(migrationAttribute.Version, migrationAttribute.Description,
                () => (IMigration) ActivatorUtilities.CreateInstance(this.serviceProvider, migrationType));
        }

        [Workspaces]
        internal class Migration : IPlainStorageEntity<long>
        {
            public long Id { get; set; }
            public string Description { get; set; }
        }
    }
}
