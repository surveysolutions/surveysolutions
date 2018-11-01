using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using FluentMigrator;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Postgres;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NConsole;
using NLog.Extensions.Logging;
using Npgsql;
using ILogger = NLog.ILogger;

namespace support
{
    [Description("Migrate Db.")]
    public class MigrateDbCommand : ConfigurationDependentCommand, IConsoleCommand
    {
        private readonly ILogger logger;

        public MigrateDbCommand(IConfigurationManagerSettings configurationManagerSettings, ILogger logger) : base(configurationManagerSettings)
        {
            this.logger = logger;
        }

        [Description("Path to the binaries of HQ application with WB.Persistence.Headquarters.dll.")]
        [Argument(Name = "bin")]
        public string BinariesPath { get; set; }

        public async Task<object> RunAsync(CommandLineProcessor processor, IConsoleHost host)
        {
            logger.Info("Migrate DB");
            if (!ReadConfigurationFile(host))
                return null;

            var dllWithMigrations = Path.Combine(BinariesPath, "WB.Persistence.Headquarters.dll");
            if (!File.Exists(dllWithMigrations))
            {
                logger.Info($"Cannot find file {dllWithMigrations}.");
                host.WriteError("bin parameter is incorrect. Please specify path to the binaries of HQ application with WB.UI.Headquarters.dll and re-run the tool.");
            }

          
            Assembly migrationsAssembly = Assembly.LoadFrom(dllWithMigrations);
            Assembly infrastructureAssembly = Assembly.LoadFrom( Path.Combine(BinariesPath, "WB.Infrastructure.Native.dll"));

            var legacyAssemblySettingsType = infrastructureAssembly.GetType("WB.Infrastructure.Native.LegacyAssemblySettings");
            dynamic legacyAssemblySettings = Activator.CreateInstance(legacyAssemblySettingsType);
            legacyAssemblySettings.FolderPath = AppDataDirectory;
            legacyAssemblySettings.AssembliesDirectoryName = "QuestionnaireAssemblies";

            var baseNamespace = "WB.Persistence.Headquarters.Migrations.{0}";
            var plainMigrationSettings = new MigrationSettings(migrationsAssembly, string.Format(baseNamespace, "PlainStore"))
            {
                SchemaName = "plainstore"
            };
            var readsideMigrationSettings = new MigrationSettings(migrationsAssembly, string.Format(baseNamespace, "ReadSide"))
            {
                SchemaName = "readside"
            };
            var eventsMigrationSettings = new MigrationSettings(migrationsAssembly, string.Format(baseNamespace, "Events"))
            {
                SchemaName = "events"
            };
            var usersMigrationSettings = new MigrationSettings(migrationsAssembly, string.Format(baseNamespace, "Users"))
            {
                SchemaName = "users"
            };

            Init(ConnectionString, eventsMigrationSettings, (serviceCollection) => { });
            Init(ConnectionString, readsideMigrationSettings, (serviceCollection) => { });
            Init(ConnectionString, plainMigrationSettings, (serviceCollection) => { serviceCollection.AddSingleton(legacyAssemblySettingsType, (provider) => legacyAssemblySettings); });
            Init(ConnectionString, usersMigrationSettings, (serviceCollection) => { });

            return null;
        }

        private void Init(string connectionString, MigrationSettings migrationSettings, Action<IServiceCollection> configureServiceCollection)
        {
            DatabaseManagement.InitDatabase(connectionString, migrationSettings.SchemaName);
            DbMigrationsRunner.MigrateToLatest(connectionString, migrationSettings, configureServiceCollection);
        }
    }


    public static class DbMigrationsRunner
    {
        public static void MigrateToLatest(string connectionString, MigrationSettings migrationSettings, Action<IServiceCollection> configureServiceCollection)
        {
            IServiceCollection serviceCollection = new ServiceCollection()
                // Logging is the replacement for the old IAnnouncer
                .AddSingleton<ILoggerProvider, NLogLoggerProvider>()
                .AddSingleton(new DefaultConventionSet(defaultSchemaName: null, workingDirectory: null))
                .Configure<ProcessorOptions>(opt => { opt.PreviewOnly = false; })
                .Configure<TypeFilterOptions>(opt => { opt.Namespace = migrationSettings.MigrationsNamespace; })
                // Registration of all FluentMigrator-specific services
                .AddFluentMigratorCore()
                // Configure the runner
                .ConfigureRunner(
                    builder => builder
                        // Add Postgres
                        .AddPostgres(migrationSettings.SchemaName)
                        // The Postgres connection string
                        .WithGlobalConnectionString(connectionString)
                        // Specify the assembly with the migrations
                        .ScanIn(migrationSettings.MigrationsAssembly)
                        .For.Migrations()
                        .For.EmbeddedResources())
                .AddLogging(lb => lb.AddFluentMigratorConsole());

            configureServiceCollection(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            // Put the database update into a scope to ensure
            // that all resources will be disposed.
            using (var scope = serviceProvider.CreateScope())
            {
                // Instantiate the runner
                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

                // Execute the migrations
                runner.MigrateUp();
            }
        }
    }

    public static class MigrationRunnerBuilderExtensions
    {

        public static IMigrationRunnerBuilder AddPostgres(this IMigrationRunnerBuilder migrationRunnerBuilder, string schemaName)
        {
            if (migrationRunnerBuilder == null) throw new ArgumentNullException(nameof(migrationRunnerBuilder));

            migrationRunnerBuilder.Services.AddScoped<PostgresDbFactory>()
                .AddScoped<PostgresProcessor>(sp => new InSchemaPostgresProcessor(schemaName,
                    sp.GetRequiredService<PostgresDbFactory>(),
                    sp.GetRequiredService<PostgresGenerator>(),
                    sp.GetRequiredService<ILogger<PostgresProcessor>>(),
                    sp.GetRequiredService<IOptionsSnapshot<ProcessorOptions>>(),
                    sp.GetRequiredService<IConnectionStringAccessor>()))
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<PostgresProcessor>())
                .AddScoped<PostgresQuoter>()
                .AddScoped<PostgresGenerator>(provider => new InSchemaPostgresGenerator(schemaName))
                .AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<PostgresGenerator>());

            return migrationRunnerBuilder;
        }
    }

    public class InSchemaPostgresGenerator : PostgresGenerator
    {
        private readonly string schemaName;

        public InSchemaPostgresGenerator(string schemaName)
        {
            this.schemaName = schemaName;
        }

        public override string Generate(AlterColumnExpression expression)
        {
            expression.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(AlterDefaultConstraintExpression expression)
        {
            expression.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(AlterTableExpression expression)
        {
            expression.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(CreateColumnExpression expression)
        {
            expression.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(CreateConstraintExpression expression)
        {
            expression.Constraint.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(CreateForeignKeyExpression expression)
        {
            expression.ForeignKey.ForeignTableSchema = this.schemaName;
            expression.ForeignKey.PrimaryTableSchema = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(CreateIndexExpression expression)
        {
            expression.Index.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(CreateSequenceExpression expression)
        {
            expression.Sequence.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(CreateTableExpression expression)
        {
            expression.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(DeleteColumnExpression expression)
        {
            expression.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(DeleteConstraintExpression expression)
        {
            expression.Constraint.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(DeleteDataExpression expression)
        {
            expression.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(DeleteDefaultConstraintExpression expression)
        {
            expression.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(DeleteForeignKeyExpression expression)
        {
            expression.ForeignKey.ForeignTableSchema = this.schemaName;
            expression.ForeignKey.PrimaryTableSchema = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(DeleteIndexExpression expression)
        {
            expression.Index.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(DeleteSequenceExpression expression)
        {
            expression.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(DeleteTableExpression expression)
        {
            expression.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(InsertDataExpression expression)
        {
            expression.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(RenameColumnExpression expression)
        {
            expression.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(RenameTableExpression expression)
        {
            expression.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(UpdateDataExpression expression)
        {
            expression.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string GetUniqueString(CreateIndexExpression column)
        {
            column.Index.SchemaName = this.schemaName;
            return base.GetUniqueString(column);
        }

        public override string GenerateForeignKeyName(ForeignKeyDefinition foreignKey)
        {
            foreignKey.ForeignTableSchema = this.schemaName;
            foreignKey.PrimaryTableSchema = this.schemaName;
            return base.GenerateForeignKeyName(foreignKey);
        }

        public override string GetClusterTypeString(CreateIndexExpression column)
        {
            column.Index.SchemaName = this.schemaName;
            return base.GetClusterTypeString(column);
        }
    }

    public class InSchemaPostgresProcessor : PostgresProcessor
    {
        public string SchemaName { get; }

        public InSchemaPostgresProcessor(string schemaName, 
            PostgresDbFactory factory, 
            PostgresGenerator generator,
            ILogger<PostgresProcessor> logger, 
            IOptionsSnapshot<ProcessorOptions> options,
            IConnectionStringAccessor connectionStringAccessor)
            : base(factory, generator, logger, options, connectionStringAccessor)
        {
            this.SchemaName = schemaName;
        }

        public override DataSet ReadTableData(string schemaName, string tableName) =>
            base.ReadTableData(this.SchemaName, tableName);

        public override bool ColumnExists(string schemaName, string tableName, string columnName) =>
            base.ColumnExists(this.SchemaName, tableName, columnName);

        public override bool TableExists(string schemaName, string tableName) =>
            base.TableExists(this.SchemaName, tableName);

        public override bool SequenceExists(string schemaName, string sequenceName) =>
            base.SequenceExists(this.SchemaName, sequenceName);

        public override bool ConstraintExists(string schemaName, string tableName, string constraintName) =>
            base.ConstraintExists(this.SchemaName, tableName, constraintName);

        public override bool IndexExists(string schemaName, string tableName, string indexName) =>
            base.IndexExists(this.SchemaName, tableName, indexName);

        public override bool DefaultValueExists(string schemaName, string tableName, string columnName,
            object defaultValue) => base.DefaultValueExists(this.SchemaName, tableName, columnName, defaultValue);
    }
    
    public class MigrationSettings
    {
        public MigrationSettings(Assembly migrationsAssembly, string migrationsNamespace)
        {
            this.MigrationsAssembly = migrationsAssembly;
            this.MigrationsNamespace = migrationsNamespace;
        }

        public Assembly MigrationsAssembly { get; private set; }

        public string MigrationsNamespace { get; private set; }

        public string SchemaName  { get; set; }
    }

    public static class DatabaseManagement
    {
        public static void InitDatabase(string connectionString, string schemaName)
        {
            CreateDatabase(connectionString);
            CreateSchema(connectionString, schemaName);
        }

        private static void CreateDatabase(string connectionString)
        {
            var masterDbConnectionString = new NpgsqlConnectionStringBuilder(connectionString);
            var databaseName = masterDbConnectionString.Database;
            masterDbConnectionString.Database = "postgres"; // System DB name.

            using (var connection = new NpgsqlConnection(masterDbConnectionString.ConnectionString))
            {
                connection.Open();
                var checkDbExistsCommand = connection.CreateCommand();
                checkDbExistsCommand.CommandText = "SELECT 1 FROM pg_catalog.pg_database WHERE lower(datname) = lower(:dbName);";
                checkDbExistsCommand.Parameters.AddWithValue("dbName", databaseName);
                var dbExists = checkDbExistsCommand.ExecuteScalar();

                if (dbExists == null)
                {
                    var createCommand = connection.CreateCommand();
                    createCommand.CommandText = $@"CREATE DATABASE ""{databaseName}"" ENCODING = 'UTF8'";
                    // unfortunately there is no way to use parameters based syntax here 
                    createCommand.ExecuteNonQuery();
                }
            }
        }

        private static void CreateSchema(string connectionString, string schemaName)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                var checkSchemaExistsCommand = connection.CreateCommand();
                checkSchemaExistsCommand.CommandText = $"CREATE SCHEMA IF NOT EXISTS {schemaName}";
                checkSchemaExistsCommand.ExecuteNonQuery();
            }
        }
    }
}
