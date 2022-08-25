using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Categories;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider.Mappings
{
    public class CategoriesInstanceTypeConfig : IEntityTypeConfiguration<CategoriesInstance>
    {
        public void Configure(EntityTypeBuilder<CategoriesInstance> builder)
        {
            builder.ToTable("categories", "plainstore");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.QuestionnaireId).HasColumnName("questionnaireid");
            builder.Property(e => e.CategoriesId).HasColumnName("categoriesid");
            builder.Property(e => e.Value).HasColumnName("value");
            builder.Property(e => e.ParentId).HasColumnName("parentid").IsRequired(false);
            builder.Property(e => e.Text).HasColumnName("text");
            builder.Property(e => e.SortIndex).HasColumnName("sortindex");
            builder.Property(e => e.AttachmentName).HasColumnName("attachment_name");

            builder.HasIndex(x => new {x.QuestionnaireId, x.CategoriesId});
        }
    }
}
