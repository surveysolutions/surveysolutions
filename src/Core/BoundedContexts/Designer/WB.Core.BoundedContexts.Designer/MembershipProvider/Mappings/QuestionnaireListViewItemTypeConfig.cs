using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using WB.Core.BoundedContexts.Designer.Implementation;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider.Mappings
{
    public class QuestionnaireListViewFolderTypeConfig : IEntityTypeConfiguration<QuestionnaireListViewFolder>
    {
        public void Configure(EntityTypeBuilder<QuestionnaireListViewFolder> builder)
        {
            builder.ToTable("questionnairelistviewfolders", "plainstore");

            builder.HasKey(x => x.PublicId);
            builder.HasIndex(e => e.Path)
                .HasName("path_questionnairelistviewfolders_idx");

            builder.Property(e => e.PublicId)
                .HasColumnName("id")
                .ValueGeneratedNever();

            builder.Property(e => e.CreateDate).HasColumnName("createdate");

            builder.Property(e => e.CreatedBy).HasColumnName("createdby");

            builder.Property(e => e.CreatorName).HasColumnName("creatorname");

            builder.Property(e => e.Depth).HasColumnName("depth");

            builder.Property(e => e.Parent).HasColumnName("parent");

            builder.Property(e => e.Path)
                .IsRequired()
                .HasColumnName("path");

            builder.Property(e => e.Title)
                .IsRequired()
                .HasColumnName("title");

            //builder.HasOne(d => d.ParentNavigation)
            //    .WithMany(p => p.InverseParentNavigation)
            //    .HasForeignKey(d => d.Parent)
            //    .OnDelete(DeleteBehavior.Cascade)
            //    .HasConstraintName("folder_relation_fk");
        }
    }

    public class SharedPersonsTypeConfig : IEntityTypeConfiguration<SharedPerson> {
        public void Configure(EntityTypeBuilder<SharedPerson> builder)
        {
            builder.ToTable("sharedpersons", "plainstore");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(e => e.Email).HasColumnName("email");
            builder.Property(e => e.QuestionnaireId).HasColumnName("questionnaireid");
            builder.Property(e => e.UserId).HasColumnName("userid");
            builder.Property(e => e.IsOwner).HasColumnName("isowner");
            builder.Property(e => e.ShareType).HasColumnName("sharetype");
            builder.HasOne(x => x.Questionnaire)
                .WithMany(x => x.SharedPersons)
                .HasForeignKey(x => x.QuestionnaireId);
        }
    }

    public class QuestionnaireListViewItemTypeConfig : IEntityTypeConfiguration<QuestionnaireListViewItem>
    {
        public void Configure(EntityTypeBuilder<QuestionnaireListViewItem> builder)
        {
            builder.ToTable("questionnairelistviewitems", "plainstore");

            builder.HasKey(x => x.QuestionnaireId);

            builder.Property(e => e.QuestionnaireId).HasColumnName("id").ValueGeneratedNever();

            builder.Ignore(e => e.PublicId);

            builder.Property(e => e.CreatedBy).HasColumnName("createdby");

            builder.Property(e => e.CreationDate).HasColumnName("creationdate");

            builder.Property(e => e.CreatorName).HasColumnName("creatorname");

            builder.Property(e => e.FolderId).HasColumnName("folderid");

            builder.Property(e => e.IsDeleted).HasColumnName("isdeleted");

            builder.Property(e => e.IsPublic).HasColumnName("ispublic");

            builder.Property(e => e.LastEntryDate).HasColumnName("lastentrydate");

            builder.Property(e => e.Owner).HasColumnName("owner");
            
            builder.Property(e => e.Title).HasColumnName("title");

            builder.HasMany(x => x.SharedPersons)
                .WithOne(x => x.Questionnaire)
                .HasForeignKey(x => x.QuestionnaireId)
                .OnDelete(DeleteBehavior.Cascade);

            //, m =>
            //{
            //    m.Key(keyMap =>
            //    {
            //        keyMap.Column(clm =>
            //        {
            //            clm.Name("QuestionnaireId");
            //        });
            //    });
            //    m.Table("SharedPersons");
            //    m.Lazy(CollectionLazy.NoLazy);
            //},
            //r => r.Component(e =>
            //{
            //    e.Property(x => x.UserId);
            //    e.Property(x => x.Email);
            //    e.Property(x => x.IsOwner);
            //    e.Property(x => x.ShareType);
            //}));
            builder.HasOne(d => d.Folder)
              .WithMany(p => p.Questionnaires)
              .HasForeignKey(d => d.FolderId)
              .OnDelete(DeleteBehavior.SetNull)
              .HasConstraintName("questionnaire_folder_relation_fk");

            //ManyToOne(x => x.Folder, m =>
            //{
            //    m.Column(nameof(QuestionnaireListViewItem.FolderId).ToLower());
            //    m.Cascade(Cascade.None);
            //    m.Update(false);
            //    m.Insert(false);
            //});
        }
    }

    public class KeyValueTableTypeConfig<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : KeyValueEntity
    {
        private readonly string tableName;

        public KeyValueTableTypeConfig(string tableName)
        {
            this.tableName = tableName;
        }

        public void Configure(EntityTypeBuilder<TEntity> builder)
        {
            builder.ToTable(tableName, "plainstore");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.Value).HasColumnName("value").HasColumnType("json");
        }
    }

    public class TranslationInstanceTypeConfig : IEntityTypeConfiguration<TranslationInstance>
    {
        public void Configure(EntityTypeBuilder<TranslationInstance> builder)
        {
            builder.ToTable("translationinstances", "plainstore");

            builder.HasKey(x => x.Id);

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            builder.Property(e => e.QuestionnaireEntityId).HasColumnName("questionnaireentityid");

            builder.Property(e => e.QuestionnaireId).HasColumnName("questionnaireid");

            builder.Property(e => e.TranslationId).HasColumnName("translationid");

            builder.Property(e => e.TranslationIndex).HasColumnName("translationindex");

            builder.Property(e => e.Type).HasColumnName("type");

            builder.Property(e => e.Value).HasColumnName("value");
        }
    }

    public class AttachmentMetaTypeConfig : IEntityTypeConfiguration<AttachmentMeta>
    {
        public void Configure(EntityTypeBuilder<AttachmentMeta> builder)
        {
            builder.ToTable("attachmentmetas", "plainstore");

            builder.HasKey(e => e.AttachmentId);

            builder.Property(e => e.AttachmentId)
                .HasColumnName("id")
                .ValueGeneratedNever();

            builder.Property(e => e.ContentId).HasColumnName("contentid");

            builder.Property(e => e.FileName).HasColumnName("filename");

            builder.Property(e => e.LastUpdateDate).HasColumnName("lastupdatedate");

            builder.Property(e => e.QuestionnaireId).HasColumnName("questionnaireid");
        }
    }

    public class QuestionnaireChangeReferenceTypeConfig : IEntityTypeConfiguration<QuestionnaireChangeReference>
    {
        public void Configure(EntityTypeBuilder<QuestionnaireChangeReference> builder)
        {
            builder.ToTable("questionnairechangereferences", "plainstore");

            builder.HasKey(x => x.Id);

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.QuestionnaireChangeRecordId)
                .HasColumnName("questionnairechangerecordid")
                .HasMaxLength(255);

            builder.Property(e => e.ReferenceId).HasColumnName("referenceid");

            builder.Property(e => e.ReferenceTitle).HasColumnName("referencetitle");

            builder.Property(e => e.ReferenceType).HasColumnName("referencetype");

            builder.HasOne(d => d.QuestionnaireChangeRecord)
                .WithMany(p => p.References)
                .HasForeignKey(d => d.QuestionnaireChangeRecordId)
                .HasConstraintName("questionnairechangereferences_questionnairechangerecordid");
        }
    }

    public class QuestionnaireChangeRecordTypeConfig : IEntityTypeConfiguration<QuestionnaireChangeRecord>
    {
        public void Configure(EntityTypeBuilder<QuestionnaireChangeRecord> builder)
        {
            builder.ToTable("questionnairechangerecords", "plainstore");

            builder.HasKey(x => x.QuestionnaireChangeRecordId);

            builder.Property(e => e.QuestionnaireChangeRecordId)
                .HasColumnName("id")
                .ValueGeneratedNever();

            builder.Property(e => e.ActionType).HasColumnName("actiontype");

            builder.Property(e => e.AffectedEntriesCount).HasColumnName("affectedentriescount");

            builder.Property(e => e.Patch).HasColumnName("patch");

            builder.Property(e => e.QuestionnaireId).HasColumnName("questionnaireid");

            builder.Property(e => e.ResultingQuestionnaireDocument).HasColumnName("resultingquestionnairedocument");

            builder.Property(e => e.Sequence).HasColumnName("sequence");

            builder.Property(e => e.TargetItemDateTime).HasColumnName("targetitemdatetime");

            builder.Property(e => e.TargetItemId).HasColumnName("targetitemid");

            builder.Property(e => e.TargetItemNewTitle).HasColumnName("targetitemnewtitle");

            builder.Property(e => e.TargetItemTitle).HasColumnName("targetitemtitle");

            builder.Property(e => e.TargetItemType).HasColumnName("targetitemtype");

            builder.Property(e => e.Timestamp).HasColumnName("timestamp");

            builder.Property(e => e.UserId).HasColumnName("userid");

            builder.Property(e => e.UserName).HasColumnName("username");
        }
    }

    public class AttachmentContentTypeConfig : IEntityTypeConfiguration<AttachmentContent>
    {
        public void Configure(EntityTypeBuilder<AttachmentContent> builder)
        {
            builder.ToTable("attachmentcontents", "plainstore");

            builder.HasKey(x => x.ContentId);
            builder.Property(e => e.ContentId)
                .HasColumnName("id")
                .ValueGeneratedNever();

            builder.OwnsOne(x => x.Details, p =>
            {
                p.Property(e => e.Height).HasColumnName("attachmentheight");

                p.Property(e => e.Width).HasColumnName("attachmentwidth");

                p.Property(e => e.Thumbnail).HasColumnName("thumbnail");

                p.ToTable(p.OwnedEntityType.ClrType.Name);
            });
            

            builder.Property(e => e.Content).HasColumnName("content");

            builder.Property(e => e.ContentType).HasColumnName("contenttype");

            builder.Property(e => e.Size).HasColumnName("size");
        }
    }
}
