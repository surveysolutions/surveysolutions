using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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
            builder.HasKey(x => x.QuestionnaireId);
            builder.HasKey(x => x.UserId);

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
            builder.ToTable("questionnairelistviewitems");

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
}
