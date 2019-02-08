using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WB.Services.Export.InterviewDataStorage.EfMappings
{
    public class InterviewQuestionnaireReferenceNodeMap : IEntityTypeConfiguration<InterviewQuestionnaireReferenceNode>
    {
        public void Configure(EntityTypeBuilder<InterviewQuestionnaireReferenceNode> builder)
        {
            builder.ToTable("interview__references");
            builder.HasKey(x => x.InterviewId);
        }
    }
}
