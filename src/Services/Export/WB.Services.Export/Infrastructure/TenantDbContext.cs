using System;
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

        public TenantDbContext(ITenantContext tenantContext, 
            IOptions<DbConnectionSettings> connectionSettings,
            DbContextOptions options) : base(options)
        {
            this.TenantContext = tenantContext;

            // failing later provide much much much more information on who and why injected this without ITenantContext
            // otherwise there will 2 step stack trace starting from one of the registered middleware
            // with zero information on who made a call
            this.connectionString = new Lazy<string>(() =>
            {
                if (tenantContext.Tenant == null) throw new ArgumentException(nameof(TenantDbContext) + " cannot be resolved outside of configured ITenantContext");

                var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionSettings.Value.DefaultConnection);
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
                optionsBuilder.UseNpgsql(connectionString.Value, b => {
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
                    return MetadataSet.Add(new Metadata{Id = key, Value = "0"}).Entity;
                }

                return meta;
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
    }
}
