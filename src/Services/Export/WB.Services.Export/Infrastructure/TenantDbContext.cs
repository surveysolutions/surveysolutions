﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using WB.Services.Export.InterviewDataStorage;
using WB.Services.Export.InterviewDataStorage.EfMappings;
using WB.Services.Infrastructure.Storage;
using WB.Services.Infrastructure.Tenant;

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

        private const long ContextSchemaVersion = 1;

        private IOptions<DbConnectionSettings> connectionSettings;

        public TenantDbContext(ITenantContext tenantContext,
            IOptions<DbConnectionSettings> connectionSettings,
            DbContextOptions options) : base(options)
        {
            this.TenantContext = tenantContext;
            this.connectionSettings = connectionSettings;

            // failing later provide much much much more information on who and why injected this without ITenantContext
            // otherwise there will 2 step stack trace starting from one of the registered middleware
            // with zero information on who made a call
            this.connectionString = new Lazy<string>(() =>
            {
                if (tenantContext.Tenant == null)
                    throw new ArgumentException(nameof(TenantDbContext) +
                                                " cannot be resolved outside of configured ITenantContext");

                var connectionStringBuilder =
                    new NpgsqlConnectionStringBuilder(connectionSettings.Value.DefaultConnection);
                connectionStringBuilder.SearchPath = tenantContext.Tenant.SchemaName();
                return connectionStringBuilder.ToString();
            });
        }

        public DbSet<InterviewReference> InterviewReferences { get; set; }
        public DbSet<Metadata> MetadataSet { get; set; }
        public DbSet<GeneratedQuestionnaireReference> GeneratedQuestionnaires { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(connectionString.Value,
                    b => { b.MigrationsHistoryTable("__migrations", this.TenantContext.Tenant.SchemaName()); });
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
                    return MetadataSet.Add(new Metadata {Id = key, Value = "0"}).Entity;
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
                    return MetadataSet.Add(new Metadata {Id = key, Value = "0"}).Entity;
                }

                return meta;
            }
        }

        public async Task CheckSchemaVersionAndMigrate()
        {
            if (await DoesSchemaExist())
            {
                long schemaVersion = 0;
                try
                {
                    schemaVersion = SchemaVersion.AsLong;
                }
                catch (Exception e)
                {
                }

                if (schemaVersion < ContextSchemaVersion)
                {
                    await DropTenantSchemaAsync(this.TenantContext.Tenant.Name);
                }
            }

            this.Database.Migrate();

            using (var tr = Database.BeginTransaction())
            {
                SchemaVersion.AsLong = ContextSchemaVersion;
                await SaveChangesAsync();
                tr.Commit();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.UseSnakeCaseNaming();

            string schema = null;
            if (!this.TenantContext.Tenant.Id.Equals(TenantId.None))
            {
                modelBuilder.HasDefaultSchema(TenantContext.Tenant.SchemaName());
                schema = TenantContext.Tenant.SchemaName();
            }

            modelBuilder.ApplyConfiguration(new InterviewReferenceEntityTypeConfiguration(schema));
            modelBuilder.ApplyConfiguration(new MetadataTypeConfiguration(schema));
            modelBuilder.ApplyConfiguration(new DeletedQuestionnaireReferenceTypeConfiguration(schema));
        }

        public async Task DropTenantSchemaAsync(string tenant, CancellationToken cancellationToken = default)
        {
            List<string> tablesToDelete = new List<string>();

            using (var db = new NpgsqlConnection(connectionSettings.Value.DefaultConnection))
            {
                await db.OpenAsync();

                //logger.LogInformation("Start drop tenant scheme: {tenant}", tenant);

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
                        new {schema});

                    foreach (var table in tables)
                    {
                        tablesToDelete.Add($@"""{schema}"".""{table}""");
                    }
                }

                foreach (var tables in tablesToDelete.Batch(30))
                {
                    using (var tr = db.BeginTransaction())
                    {
                        foreach (var table in tables)
                        {
                            await db.ExecuteAsync($@"drop table if exists {table}");
                            //logger.LogInformation("Dropped {table}", table);
                        }

                        await tr.CommitAsync();
                    }
                }

                using (var tr = db.BeginTransaction())
                {
                    foreach (var schema in schemas)
                    {
                        await db.ExecuteAsync($@"drop schema if exists ""{schema}""");
                        //logger.LogInformation("Dropped schema {schema}.", schema);
                    }

                    await tr.CommitAsync();
                }
            }
        }

        private async Task<bool> DoesSchemaExist()
        {
            using (var db = new NpgsqlConnection(connectionSettings.Value.DefaultConnection))
            {
                await db.OpenAsync();
                var name = TenantContext.Tenant.SchemaName();
                var exists = await db.QueryFirstAsync<bool>(
                    "SELECT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = @name);",
                    new
                    {
                        name
                    });
                return exists;
            }
        }
    }
}
