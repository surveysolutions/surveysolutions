using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WB.Core.BoundedContexts.Designer.Comments;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider.Mappings
{
    public class CommentInstanceTypeConfig : IEntityTypeConfiguration<CommentInstance>
    {
        public void Configure(EntityTypeBuilder<CommentInstance> builder)
        {
            builder.ToTable("commentinstances", "plainstore");

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            builder.Property(e => e.Comment)
                .IsRequired()
                .HasColumnName("comment");

            builder.Property(e => e.Date).HasColumnName("date");

            builder.Property(e => e.EntityId).HasColumnName("entityid");

            builder.Property(e => e.QuestionnaireId).HasColumnName("questionnaireid");

            builder.Property(e => e.ResolveDate).HasColumnName("resolvedate").IsRequired(false);

            builder.Property(e => e.UserEmail)
                .IsRequired()
                .HasColumnName("useremail");

            builder.Property(e => e.UserName)
                .IsRequired()
                .HasColumnName("username");
        }
    }
}
