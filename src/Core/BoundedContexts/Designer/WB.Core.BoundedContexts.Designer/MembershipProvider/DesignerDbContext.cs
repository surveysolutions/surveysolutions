using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WB.Core.BoundedContexts.Designer.MembershipProvider.Mappings;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider
{
    public class DesignerDbContext : IdentityDbContext<DesignerIdentityUser>
    {
        public DesignerDbContext(DbContextOptions options) : base(options)
        {
        }

        protected DesignerDbContext()
        {
        }

        public DbSet<QuestionnaireListViewItem> Questionnaires { get; set;  }

        public DbSet<QuestionnaireListViewFolder> QuestionnaireFolders { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.HasDefaultSchema("plainstore");

            builder.Entity<DesignerIdentityUser>(x => x.Property(p => p.PasswordSalt).HasColumnName("PasswordSalt"));
            builder.Entity<DesignerIdentityUser>(x => x.Property(p => p.CanImportOnHq).HasColumnName("CanImportOnHq"));
            builder.Entity<DesignerIdentityUser>(x => x.Property(p => p.CreatedAtUtc).HasColumnName("CreatedAtUtc"));
            builder.ApplyConfiguration(new QuestionnaireListViewItemTypeConfig());
            builder.ApplyConfiguration(new QuestionnaireListViewFolderTypeConfig());
            builder.ApplyConfiguration(new SharedPersonsTypeConfig());
        }
    }
}
