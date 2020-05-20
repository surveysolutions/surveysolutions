using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider.Mappings
{
    public class QuestionnaireListViewItemTypeConfig : IEntityTypeConfiguration<QuestionnaireListViewItem>
    {
        public void Configure(EntityTypeBuilder<QuestionnaireListViewItem> builder)
        {
            builder.ToTable("questionnairelistviewitems", "plainstore");

            builder.HasKey(x => x.QuestionnaireId);

            builder.Property(e => e.QuestionnaireId).HasColumnName("id").ValueGeneratedNever();

            builder.Property(e => e.PublicId).HasColumnName("publicid");

            builder.Property(e => e.CreatedBy).IsRequired(false).HasColumnName("createdby");

            builder.Property(e => e.CreationDate).HasColumnName("creationdate");

            builder.Property(e => e.CreatorName).HasColumnName("creatorname");

            builder.Property(e => e.FolderId).IsRequired(false).HasColumnName("folderid");

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
