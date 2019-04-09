using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Implementation;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.MembershipProvider.Mappings;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.Infrastructure.PlainStorage;

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

        public DbSet<AttachmentContent> AttachmentContents { get; set; }

        public DbSet<AttachmentMeta> AttachmentMetas { get; set; }
        public DbSet<TranslationInstance> TranslationInstances { get; set; }

        public DbSet<StoredLookupTable> LookupTableContents { get; set; }

        public DbSet<StoredQuestionnaireDocument> QuestionnaireDocuments { get; set; }

        public DbSet<StoredQuestionnaireStateTracker> QuestionnaireStateTrackers { get; set; }

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
            builder.ApplyConfiguration(new AttachmentContentTypeConfig());
            builder.ApplyConfiguration(new AttachmentMetaTypeConfig());
            builder.ApplyConfiguration(new TranslationInstanceTypeConfig());

            // Key value
            builder.ApplyConfiguration(new KeyValueTableTypeConfig<StoredLookupTable>("lookuptablecontents"));
            builder.ApplyConfiguration(new KeyValueTableTypeConfig<StoredQuestionnaireDocument>("questionnairedocuments"));
            builder.ApplyConfiguration(new KeyValueTableTypeConfig<StoredQuestionnaireStateTracker>("questionnairestatetrackers"));
        }
    }
}
