using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WB.Services.Export.InterviewDataStorage.EfMappings
{
    public class InterviewSummaryEntityTypeConfiguration : IEntityTypeConfiguration<InterviewSummary>
    {
        public void Configure(EntityTypeBuilder<InterviewSummary> builder)
        {
            builder.ToTable("interview__references");
            builder.HasKey(x => x.InterviewId);
        }
    }
}
