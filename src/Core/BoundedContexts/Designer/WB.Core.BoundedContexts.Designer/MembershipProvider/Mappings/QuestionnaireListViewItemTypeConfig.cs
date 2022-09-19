using System;
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

            builder.Property(e => e.OwnerId).IsRequired(false).HasColumnName("ownerid");
            
            builder.Property(e => e.CreationDate).HasColumnName("creationdate");

            builder.Property(e => e.CreatorName).HasColumnName("creatorname");

            builder.Property(e => e.FolderId).IsRequired(false).HasColumnName("folderid");

            builder.Property(e => e.IsDeleted).HasColumnName("isdeleted");

            builder.Property(e => e.IsPublic).HasColumnName("ispublic");

            builder.Property(e => e.LastEntryDate).HasColumnName("lastentrydate");

            builder.Property(e => e.Owner).HasColumnName("owner");
            
            builder.Property(e => e.Title).IsRequired(false).HasColumnName("title");

            builder.HasMany(x => x.SharedPersons)
                .WithOne(x => x.Questionnaire)
                .HasForeignKey(x => x.QuestionnaireId)
                .OnDelete(DeleteBehavior.Cascade);

            var meta = builder.Metadata
                .FindNavigation(nameof(QuestionnaireListViewItem.SharedPersons));
            if (meta == null)
                throw new Exception("Invalid state. Metadata was not found");
                
            meta.SetPropertyAccessMode(PropertyAccessMode.Field);

             // builder.UsePropertyAccessMode(PropertyAccessMode.PreferField);
            
            builder.HasOne(d => d.Folder)
                .WithMany()
                .HasForeignKey(d => d.FolderId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("questionnaire_folder_relation_fk");
            
        }
    }
}
