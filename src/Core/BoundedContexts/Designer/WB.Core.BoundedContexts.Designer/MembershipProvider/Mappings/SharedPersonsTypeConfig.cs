using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider.Mappings
{
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
            
            var meta= builder.Metadata
                .FindNavigation(nameof(SharedPerson.Questionnaire));

            if (meta == null) throw new InvalidOperationException("Invalid meta state.");
            meta.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
