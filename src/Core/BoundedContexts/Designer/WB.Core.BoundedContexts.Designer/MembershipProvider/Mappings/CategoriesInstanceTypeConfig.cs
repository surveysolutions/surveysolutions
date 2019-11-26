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
            
            builder.Property(e => e.QuestionnaireId).HasColumnName("questionnaireid");
            builder.Property(e => e.CategoriesId).HasColumnName("categoriesid");
            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.ParentId).HasColumnName("parentid");
            builder.Property(e => e.Text).HasColumnName("text");
        }
    }
}
