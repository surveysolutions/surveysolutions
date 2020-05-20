using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;

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

            builder.Property(e => e.CreatorName).HasColumnName("creatorname").IsRequired(false);

            builder.Property(e => e.Depth).HasColumnName("depth");

            builder.Property(e => e.Parent).HasColumnName("parent").IsRequired(false);

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
}
