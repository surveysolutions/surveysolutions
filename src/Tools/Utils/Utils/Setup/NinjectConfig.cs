using System.Collections.Generic;
using System.Reflection;
using Ninject;
using Utils.Commands;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.SharedKernels.DataCollection;
using WB.Infrastructure.Native.Logging;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre;

namespace Utils.Setup
{
    public class NinjectConfig
    {
        public static IKernel CreateKernelForDebug()
        {
            var kernel = new StandardKernel(
                new NinjectSettings {InjectNonPublic = true},
                new NcqrsModule().AsNinject(),
                new NLogLoggingModule().AsNinject(),
                new ServiceLocationModule(),
                new EventSourcedInfrastructureModule().AsNinject(),
                new InfrastructureModule().AsNinject(),
                new DataCollectionSharedKernelModule().AsNinject(),
                new CoreDebugModule().AsNinject());
            return kernel;
        }

        public static IKernel CreateKernel(string connectionString)
        {
            var cacheSettings = new ReadSideCacheSettings(1024, 512);
            var mappingAssemblies = new List<Assembly> {typeof(HeadquartersBoundedContextModule).Assembly};

            var eventStoreSettings = new PostgreConnectionSettings
            {
                ConnectionString = connectionString,
                SchemaName = "events"
            };

            var postgresPlainStorageSettings = new PostgresPlainStorageSettings
            {
                ConnectionString = connectionString,
                SchemaName = "plainstore",
                DbUpgradeSettings =
                    new DbUpgradeSettings(typeof(CoreTestRunner).Assembly, typeof(CoreTestRunner).Namespace),
                MappingAssemblies = new List<Assembly>
                {
                    typeof(HeadquartersBoundedContextModule).Assembly
                }
            };

            var kernel = new StandardKernel(
                new NinjectSettings {InjectNonPublic = true},
                new NcqrsModule().AsNinject(),
                new NLogLoggingModule().AsNinject(),
                new ServiceLocationModule(),
                new EventSourcedInfrastructureModule().AsNinject(),
                new InfrastructureModule().AsNinject(),
                new DataCollectionSharedKernelModule().AsNinject(),
                new PostgresKeyValueModule(cacheSettings).AsNinject(),
                new PostgresReadSideModule(
                    connectionString,
                    PostgresReadSideModule.ReadSideSchemaName,
                    new DbUpgradeSettings(typeof(CoreTestRunner).Assembly, typeof(CoreTestRunner).Namespace),
                    cacheSettings,
                    mappingAssemblies,
                    runInitAndMigrations: false
                ).AsNinject(),
                new PostgresPlainStorageModule(postgresPlainStorageSettings).AsNinject(),
                new CoreTesterModule(eventStoreSettings).AsNinject());
            return kernel;
        }
    }
}