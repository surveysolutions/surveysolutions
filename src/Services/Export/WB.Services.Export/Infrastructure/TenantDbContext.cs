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
        private readonly string connectionString;

        public TenantDbContext(ITenantContext tenantContext, 
            IOptions<DbConnectionSettings> connectionSettings,
            DbContextOptions options) : base(options)
        {
            this.tenantContext = tenantContext;
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionSettings.Value.DefaultConnection);
            connectionStringBuilder.SearchPath = tenantContext.Tenant.SchemaName();
            this.connectionString = connectionStringBuilder.ToString();
        }

        public DbSet<InterviewReference> InterviewReferences { get; set; }
        public DbSet<Metadata> MetadataSet { get; set; }
        public DbSet<DeletedQuestionnaireReference> DeletedQuestionnaires { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(connectionString, b => {
                    b.MigrationsHistoryTable("__migrations", this.tenantContext.Tenant.SchemaName()); 
                });
            }
        }

        public Metadata Metadata
        {
            get
            {
                
                var meta = MetadataSet.Find("settings");
                if (meta == null)
                {
                    return MetadataSet.Add(new Metadata{Id = "settings"}).Entity;
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
