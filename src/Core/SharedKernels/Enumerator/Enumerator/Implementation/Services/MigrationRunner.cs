using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using MvvmCross.Binding.BindingContext;
using SQLite;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Implementation.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using WB.Core.SharedKernels.Enumerator.Views;

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

        public void MigrateUp(string appName, Assembly[] scanInAssembly)
        {
            FixMigrationRecords(scanInAssembly, appName);
                
            using var workspacesLifetimeScope = lifetimeScope.BeginLifetimeScope(cb =>
            {
                cb.RegisterGeneric(typeof(SqlitePlainStorage<,>)).As(typeof(IPlainStorage<,>)).SingleInstance();
                cb.RegisterGeneric(typeof(SqlitePlainStorage<>)).As(typeof(IPlainStorage<>)).SingleInstance();
            });
            
            workspacesLifetimeScope.Resolve<MigrationRunner>().Migrate(scanInAssembly, 
                "workspaces",
                new HashSet<string>()
                {
                    "WB.UI.Shared.Enumerator.Migrations.Workspaces",
                    $"WB.UI.{appName}.Migrations.Workspaces",
                });
            

            var workspaces = workspaceService.GetAll();
            foreach (var workspace in workspaces)
            {
                var workspaceAccessor = new SingleWorkspaceAccessor(workspace.Name);
                using var workspaceLifetimeScope = lifetimeScope.BeginLifetimeScope(cb =>
                {
                    cb.Register(c => workspaceAccessor).As<IWorkspaceAccessor>().SingleInstance();
                    cb.RegisterGeneric(typeof(SqlitePlainStorageWithWorkspace<,>)).As(typeof(IPlainStorage<,>)).SingleInstance();
                    cb.RegisterGeneric(typeof(SqlitePlainStorageWithWorkspace<>)).As(typeof(IPlainStorage<>)).SingleInstance();

                    cb.RegisterType<AssignmentDocumentsStorage>().As<IAssignmentDocumentsStorage>().SingleInstance();
                    cb.RegisterType<CalendarEventStorage>().As<ICalendarEventStorage>().SingleInstance();
                });
                workspaceLifetimeScope.Resolve<MigrationRunner>().Migrate(scanInAssembly,
                    workspace.Name,
                    new HashSet<string>()
                    {
                        "WB.UI.Shared.Enumerator.Migrations.Workspace",
                        $"WB.UI.{appName}.Migrations.Workspace",
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
            [PrimaryKey]
            public long Id { get; set; }
            public string Description { get; set; }
        }
        
        public void FixMigrationRecords(Assembly[] scanInAssembly, string appName)
        {
            // fix bug in 21.05.6 when migration records were stored in incorrect place
            var rootMigrationsRepository = new SqlitePlainStorage<Migration, long>(
                lifetimeScope.Resolve<ILogger>(),
                lifetimeScope.Resolve<IFileSystemAccessor>(),
                lifetimeScope.Resolve<SqliteSettings>());

            const long fixMigrationId = 202106141328;
            
            if (rootMigrationsRepository.Count(m => m.Id == fixMigrationId) == 1)
                return;
            
            var nonExecutedRootMigrationInfos = scanInAssembly
                .SelectMany(ass => this.LoadMigrations(ass, new HashSet<string>()
                {
                    "WB.UI.Shared.Enumerator.Migrations.Workspaces",
                    $"WB.UI.{appName}.Migrations.Workspaces",
                }))
                .Where(x => rootMigrationsRepository.Count(y => y.Id == x.Key) == 0)
                .ToArray();

            var workspaces = workspaceService.GetAll().ToList();
            if (workspaces.All(w => w.Name != "primary"))
                workspaces.Add(new WorkspaceView() { Id = "primary" });   
            
            foreach (var workspace in workspaces)
            {
                var migrationsRepositoryWithPossibleInfo = new SqlitePlainStorageWithWorkspace<Migration, long>(
                    lifetimeScope.Resolve<ILogger>(),
                    lifetimeScope.Resolve<IFileSystemAccessor>(),
                    lifetimeScope.Resolve<SqliteSettings>(),
                    workspaceAccessor: new SingleWorkspaceAccessor(workspace.Name));

                foreach (var migrationInfo in nonExecutedRootMigrationInfos)
                {
                    var migration = migrationsRepositoryWithPossibleInfo.Where(y => y.Id == migrationInfo.Key);
                    if (migration.Count > 0)
                    {
                        rootMigrationsRepository.Store(migration);
                        migrationsRepositoryWithPossibleInfo.Remove(migration);
                    }
                }
            }

            Migration fixMigrationInfo = new Migration()
            {
                Id = fixMigrationId, 
                Description = "Move migration info from inside workspace db"
            };
            rootMigrationsRepository.Store(fixMigrationInfo);
        }
    }
}
