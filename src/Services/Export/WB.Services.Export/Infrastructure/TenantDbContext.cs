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
        private readonly ITenantContext tenantContext;
        private readonly Lazy<string> connectionString;

        public TenantDbContext(ITenantContext tenantContext, 
            IOptions<DbConnectionSettings> connectionSettings,
            DbContextOptions options) : base(options)
        {
            this.tenantContext = tenantContext;

            // failing later provide much much much more information on who and why injected this without ITenantContext
            // otherwise there will 2 step stack trace starting from one of the registered middlewares
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
                    b.MigrationsHistoryTable("__migrations", this.tenantContext.Tenant.SchemaName()); 
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

            if (!this.tenantContext.Tenant.Id.Equals(TenantId.None))
            {
                modelBuilder.HasDefaultSchema(tenantContext.Tenant.SchemaName());
            }

            modelBuilder.ApplyConfiguration(new InterviewReferenceEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new DeletedQuestionnaireReferenceTypeConfiguration());
        }
    }
}
