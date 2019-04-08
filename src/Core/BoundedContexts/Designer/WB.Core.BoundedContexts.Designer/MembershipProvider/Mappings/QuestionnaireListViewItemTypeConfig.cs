using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider.Mappings
{
    public class QuestionnaireListViewFolderTypeConfig : IEntityTypeConfiguration<QuestionnaireListViewFolder>
    {
        public void Configure(EntityTypeBuilder<QuestionnaireListViewFolder> builder)
        {
            builder.ToTable("questionnairelistviewfolders");
            builder.HasKey(x => x.PublicId);

            builder.Property(x => x.Title);
            builder.Property(x => x.PublicId).HasColumnName("id");
            builder.Property(x => x.Parent);
            builder.Property(x => x.CreateDate);
            builder.Property(x => x.CreatedBy);
            builder.Property(x => x.Path);
            builder.Property(x => x.Depth);
            builder.Property(x => x.CreatorName);
        }
    }

    public class QuestionnaireListViewItemTypeConfig : IEntityTypeConfiguration<QuestionnaireListViewItem>
    {
        public void Configure(EntityTypeBuilder<QuestionnaireListViewItem> builder)
        {
            builder.ToTable("questionnairelistviewitems");

            builder.HasKey(x => x.QuestionnaireId);
            builder.Property(x => x.QuestionnaireId);
            builder.Property(x => x.PublicId);
            builder.Property(x => x.CreationDate);
            builder.Property(x => x.LastEntryDate);
            builder.Property(x => x.Title);
            builder.Property(x => x.CreatedBy);
            builder.Property(x => x.CreatorName);
            builder.Property(x => x.IsDeleted);
            builder.Property(x => x.IsPublic);
            builder.Property(x => x.Owner);
            builder.Property(x => x.FolderId);

            builder.HasMany(x => x.SharedPersons);
                
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
                builder.HasOne(x => x.Folder);

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
