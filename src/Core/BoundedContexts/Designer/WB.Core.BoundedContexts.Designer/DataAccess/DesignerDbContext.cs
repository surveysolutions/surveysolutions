using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Classifications;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Categories;
using WB.Core.BoundedContexts.Designer.Comments;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.MembershipProvider.Mappings;
using WB.Core.BoundedContexts.Designer.QuestionnaireCompilationForOldVersions;
using WB.Core.BoundedContexts.Designer.Scenarios;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.Infrastructure.Versions;

namespace WB.Core.BoundedContexts.Designer.DataAccess
{
    public class DesignerDbContext : IdentityDbContext<DesignerIdentityUser, DesignerIdentityRole, Guid>
    {
#pragma warning disable 8618
        public DesignerDbContext(DbContextOptions options) : base(options)
#pragma warning restore 8618
        {
        }

#pragma warning disable 8618
        protected DesignerDbContext()
#pragma warning restore 8618
        {
        }

        public DbSet<QuestionnaireListViewItem> Questionnaires { get; set;  }

        public DbSet<QuestionnaireListViewFolder> QuestionnaireFolders { get; set; }

        public DbSet<AttachmentContent> AttachmentContents { get; set; }

        public DbSet<AttachmentMeta> AttachmentMetas { get; set; }

        public DbSet<TranslationInstance> TranslationInstances { get; set; }

        public DbSet<CategoriesInstance> CategoriesInstances { get; set; }

        public DbSet<CommentInstance> CommentInstances { get; set; }

        public DbSet<ClassificationEntity> ClassificationEntities { get; set; }

        public DbSet<QuestionnaireChangeRecord> QuestionnaireChangeRecords { get; set; }

        public DbSet<SharedPerson> SharedPersons { get; set; }

        public DbSet<QuestionnaireCompilationVersion> QuestionnaireCompilationVersions { get; set; }

        public DbSet<QuestionnaireChangeReference> QuestionnaireChangeReferences { get; set; }

        public DbSet<StoredLookupTable> LookupTableContents { get; set; }

        public DbSet<StoredQuestionnaireDocument> QuestionnaireDocuments { get; set; }

        public DbSet<StoredQuestionnaireStateTracker> QuestionnaireStateTrackers { get; set; }

        public DbSet<ProductVersionChange> ProductVersionChanges { get; set; }

        public DbSet<StoredScenario> Scenarios { get; set; }

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
            builder.ApplyConfiguration(new QuestionnaireChangeRecordTypeConfig());
            builder.ApplyConfiguration(new QuestionnaireChangeReferenceTypeConfig());
            builder.ApplyConfiguration(new QuestionnaireCompilationVersionTypeConfig());
            builder.ApplyConfiguration(new CommentInstanceTypeConfig());
            builder.ApplyConfiguration(new ClassificationEntityTypeConfig());
            builder.ApplyConfiguration(new ProductVersionChangeTypeConfig());
            builder.ApplyConfiguration(new StoredScenarioTypeConfig());
            builder.ApplyConfiguration(new CategoriesInstanceTypeConfig());

            // Key value
            builder.ApplyConfiguration(new KeyValueTableTypeConfig<StoredLookupTable>("lookuptablecontents"));
            builder.ApplyConfiguration(new KeyValueTableTypeConfig<StoredQuestionnaireDocument>("questionnairedocuments"));
            builder.ApplyConfiguration(new KeyValueTableTypeConfig<StoredQuestionnaireStateTracker>("questionnairestatetrackers"));
        }
    }
}
