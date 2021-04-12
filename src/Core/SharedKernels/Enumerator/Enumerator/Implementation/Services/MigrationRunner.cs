using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using MvvmCross.Binding.BindingContext;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    internal class MigrationRunner : IMigrationRunner
    {
        private readonly ILifetimeScope lifetimeScope;
        private readonly IServiceProvider serviceProvider;
        private readonly IPlainStorage<Migration, long> migrationsRepository;
        private readonly ILogger logger;
        private readonly IWorkspaceService workspaceService;

        public MigrationRunner(ILifetimeScope lifetimeScope, IServiceProvider serviceProvider, 
            IPlainStorage<Migration, long> migrationsRepository, ILogger logger,
            IWorkspaceService workspaceService)
        {
            this.lifetimeScope = lifetimeScope;
            this.serviceProvider = serviceProvider;
            this.migrationsRepository = migrationsRepository;
            this.logger = logger;
            this.workspaceService = workspaceService;
        }

        public void MigrateUp(Assembly[] scanInAssembly)
        {
            using var workspacesLifetimeScope = lifetimeScope.BeginLifetimeScope(cb =>
            {
                cb.RegisterGeneric(typeof(SqlitePlainStorage<>)).As(typeof(IPlainStorage<,>));
                cb.RegisterGeneric(typeof(SqlitePlainStorage<>)).As(typeof(IPlainStorage<>));
            });
            
            workspacesLifetimeScope.Resolve<MigrationRunner>().Migrate(scanInAssembly, 
                "workspaces",
                new HashSet<string>()
                {
                    "WB.UI.Shared.Enumerator.Migrations.Workspaces",
                    "WB.UI.Interviewer.Migrations.Workspaces"   
                });
            

            var workspaces = workspaceService.GetAll();
            foreach (var workspace in workspaces)
            {
                var workspaceAccessor = new SingleWorkspaceAccessor(workspace);
                using var workspaceLifetimeScope = lifetimeScope.BeginLifetimeScope(cb =>
                {
                    cb.Register(c => workspaceAccessor).As<IWorkspaceAccessor>().SingleInstance();
                });
                workspaceLifetimeScope.Resolve<MigrationRunner>().Migrate(scanInAssembly,
                    workspace.Name,
                    new HashSet<string>()
                    {
                        "WB.UI.Shared.Enumerator.Migrations.Workspace",
                        "WB.UI.Interviewer.Migrations.Workspace"
                    });
            }
        }

        private void Migrate(Assembly[] scanInAssembly, string workspaceDescription, HashSet<string> migrationNamespaces)
        {
            var migrationInfos = scanInAssembly.SelectMany(ass => this.LoadMigrations(ass, migrationNamespaces))
                .Where(x => this.migrationsRepository.Count(y => y.Id == x.Key) == 0)
                .Select(x => x.Value)
                .ToArray();

            this.logger.Trace($"Migrations {workspaceDescription}. {migrationInfos.Length} new migration(s) found");

            foreach (var migrationInfo in migrationInfos)
            {
                var migration = migrationInfo.Migration;
                var migrationDescription = migrationInfo.Description ?? migration.GetType().Name;

                this.logger.Debug($"Migrations {workspaceDescription}. Migration: {migrationDescription}({migrationInfo.Version}) started");

                Stopwatch sw = Stopwatch.StartNew();

                migration.Up();

                this.migrationsRepository.Store(new Migration
                {
                    Id = migrationInfo.Version,
                    Description = migrationDescription
                });

                this.logger.Debug(
                    $"Migrations {workspaceDescription}. Migration: {migrationDescription}({migrationInfo.Version}) completed. Took {sw.Elapsed}");
            }
        }

        public SortedList<long, IMigrationInfo> LoadMigrations(Assembly scanInAssembly, HashSet<string> migrationNamespaces)
        {
            var sortedMigrations = new SortedList<long, IMigrationInfo>();

            var migrations = scanInAssembly.GetExportedTypes()
                .Where(type =>
                    !type.IsAbstract 
                    && typeof(IMigration).IsAssignableFrom(type) 
                    && type.GetCustomAttributes<MigrationAttribute>().Any()
                    && migrationNamespaces.Contains(type.Namespace))
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

        internal class Migration : IPlainStorageEntity<long>
        {
            public long Id { get; set; }
            public string Description { get; set; }
        }
    }
}
