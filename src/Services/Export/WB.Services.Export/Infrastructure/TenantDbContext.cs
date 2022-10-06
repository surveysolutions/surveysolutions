using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using WB.Services.Export.Assignment;
using WB.Services.Export.InterviewDataStorage;
using WB.Services.Export.InterviewDataStorage.EfMappings;
using WB.Services.Infrastructure.Storage;
using WB.Services.Infrastructure.Tenant;
using Microsoft.Extensions.Logging;

namespace WB.Services.Export.Infrastructure
{
    public class TenantDbContext : DbContext
    {
        /// <summary>
        /// Used to access tenant context from <see cref="TenantModelCacheKeyFactory"/>.
        /// For some unknown reason it is not possible to inject <see cref="ITenantContext"/> directly into <see cref="TenantModelCacheKeyFactory"/>.
        /// When done so `IsNpgsql` extension throws exception that <see cref="ITenantContext"/> con not be resolved.
        /// </summary>
        /// <example>
        /// https://weblogs.thinktecture.com/pawel/2018/06/entity-framework-core-changing-database-schema-at-runtime.html
        /// </example>
        public ITenantContext TenantContext { get; }

        private readonly Lazy<string> connectionString;

        private const long ContextSchemaVersion = 6;

        private readonly IOptions<DbConnectionSettings> connectionSettings;
        private readonly ILogger<TenantDbContext>? logger;

        public TenantDbContext(ITenantContext tenantContext,
            IOptions<DbConnectionSettings> connectionSettings,
            DbContextOptions options,
            ILogger<TenantDbContext>? logger = null) : base(options)
        {
            this.TenantContext = tenantContext;
            this.connectionSettings = connectionSettings;
            this.logger = logger;

            // failing later provide much much much more information on who and why injected this without ITenantContext
            // otherwise there will 2 step stack trace starting from one of the registered middleware
            // with zero information on who made a call
            this.connectionString = new Lazy<string>(() =>
            {
                return connectionStringCache.GetOrAdd(tenantContext.Tenant.SchemaName(), tenant =>
                {
                    if (tenantContext.Tenant == null)
                        throw new ArgumentException($"{nameof(TenantDbContext)} cannot be resolved outside of configured ITenantContext");

                    var connectionStringBuilder =
                        new NpgsqlConnectionStringBuilder(connectionSettings.Value.DefaultConnection)
                        {
                            Enlist = false,
                            SearchPath = tenantContext.Tenant.SchemaName()
                        };

                    return connectionStringBuilder.ToString();
                });
            });
        }

        private static readonly ConcurrentDictionary<string, string> connectionStringCache = new ConcurrentDictionary<string, string>();

        public DbSet<InterviewReference> InterviewReferences { get; set; } = null!;
        public DbSet<Metadata> MetadataSet { get; set; } = null!;
        public DbSet<GeneratedQuestionnaireReference> GeneratedQuestionnaires { get; set; } = null!;
        public DbSet<AssignmentAction> AssignmentActions { get; set; } = null!;
        public DbSet<Assignment.Assignment> Assignments { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(connectionString.Value,
                    b =>
                    {
                        b.MigrationsHistoryTable("__migrations", this.TenantContext.Tenant.SchemaName());
                    });
            }
        }

        public Metadata GlobalSequence
        {
            get
            {
                const string key = "globalSequence";

                var meta = MetadataSet.Find(key);
                if (meta == null)
                {
                    return MetadataSet.Add(new Metadata { Id = key, Value = "0" }).Entity;
                }

                return meta;
            }
        }

        public Metadata SchemaVersion
        {
            get
            {
                const string key = "schemaVersion";

                var meta = MetadataSet.Find(key);
                if (meta == null)
                {
                    return MetadataSet.Add(new Metadata { Id = key, Value = "0" }).Entity;
                }

                return meta;
            }
        }

        private async Task CheckSchemaVersionAndMigrate(CancellationToken cancellationToken)
        {
            if (await DoesSchemaVersionTableExists(cancellationToken))
            {
                long schemaVersion = 0;
                schemaVersion = SchemaVersion.AsLong;
                
                if (schemaVersion < ContextSchemaVersion)
                {
                    await DropTenantSchemaAsync(this.TenantContext.Tenant.Name, cancellationToken);
                }
            }

            var s = this.Database.GetConnectionString();
            await this.Database.MigrateAsync(cancellationToken: cancellationToken);
        }

        private async Task<bool> DoesSchemaVersionTableExists(CancellationToken cancellationToken)
        {
            await using var db = new NpgsqlConnection(connectionSettings.Value.DefaultConnection);

            await db.OpenAsync(cancellationToken);
            var name = TenantContext.Tenant.SchemaName();
            var exists = await db.QueryFirstAsync<bool>(
                @"SELECT EXISTS (SELECT FROM information_schema.tables 
                    WHERE  table_schema = @schemaName
                    AND    table_name   = @tableName)",
                new
                {
                    schemaName = name,
                    tableName = "metadata"
                });
            return exists;
        }

        private async Task SetContextSchema(CancellationToken cancellationToken)
        {
            await using var tr = await Database.BeginTransactionAsync(cancellationToken);

            SchemaVersion.AsLong = ContextSchemaVersion;
            await SaveChangesAsync(cancellationToken);
            await tr.CommitAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.UseSnakeCaseNaming();

            string? schema = null;
            if (!this.TenantContext.Tenant.Id.Equals(TenantId.None))
            {
                var schemaName = TenantContext.Tenant.SchemaName();
                modelBuilder.HasDefaultSchema(schemaName);
                schema = schemaName;
            }

            modelBuilder.ApplyConfiguration(new InterviewReferenceEntityTypeConfiguration(schema));
            modelBuilder.ApplyConfiguration(new MetadataTypeConfiguration(schema));
            modelBuilder.ApplyConfiguration(new DeletedQuestionnaireReferenceTypeConfiguration(schema));
            modelBuilder.ApplyConfiguration(new AssignmentTypeConfiguration(schema));
            modelBuilder.ApplyConfiguration(new AssignmentActionTypeConfiguration(schema));
        }

        public async Task DropTenantSchemaAsync(string tenant, CancellationToken cancellationToken = default)
        {
            List<string> tablesToDelete = new List<string>();

            await using var db = new NpgsqlConnection(connectionSettings.Value.DefaultConnection);
            await db.OpenAsync(cancellationToken);

            logger?.LogInformation("Start drop tenant schema: {Tenant}", tenant);

            var schemas = (await db.QueryAsync<string>(
                "select nspname from pg_catalog.pg_namespace n " +
                "join pg_catalog.pg_description d on d.objoid = n.oid " +
                "where d.description = @tenant",
                new
                {
                    tenant
                })).ToList();

            foreach (var schema in schemas)
            {
                var tables = await db.QueryAsync<string>(
                    "select tablename from pg_tables where schemaname= @schema",
                    new { schema });

                foreach (var table in tables)
                {
                    tablesToDelete.Add($@"""{schema}"".""{table}""");
                }
            }

            foreach (var tables in tablesToDelete.Batch(10))
            {
                await using var tr = await db.BeginTransactionAsync(cancellationToken);
                foreach (var table in tables)
                {
                    await db.ExecuteAsync($@"drop table if exists {table}");
                    logger?.LogInformation("Dropped {Table}", table);
                }

                await tr.CommitAsync(cancellationToken);
            }

            await using (var tr = await db.BeginTransactionAsync(cancellationToken))
            {
                foreach (var schema in schemas)
                {
                    await db.ExecuteAsync($@"drop schema if exists ""{schema}""");
                    logger?.LogInformation("Dropped schema {Schema}", schema);
                }

                await tr.CommitAsync(cancellationToken);
            }
        }

        public async Task EnsureMigrated(CancellationToken cancellationToken)
        {
            if (Database.IsNpgsql())
            {
                await CheckSchemaVersionAndMigrate(cancellationToken);
                await SetContextSchema(cancellationToken);
            }
        }
    }
}
