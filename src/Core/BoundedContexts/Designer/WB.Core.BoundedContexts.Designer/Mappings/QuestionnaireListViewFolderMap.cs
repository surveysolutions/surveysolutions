using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Mappings
{
    [PlainStorage]
    public class QuestionnaireListViewFolderMap : ClassMapping<QuestionnaireListViewFolder>
    {
        public QuestionnaireListViewFolderMap()
        {
            Id(x => x.PublicId, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });
            DynamicUpdate(true);

            Property(x => x.Title);
            Property(x => x.Parent);
            Property(x => x.CreateDate);
            Property(x => x.CreatedBy);
            Property(x => x.Path);
            Property(x => x.Depth);

            Set(x => x.SubFolders, collection => {
                collection.Key(c => {
                    c.Column(nameof(QuestionnaireListViewFolder.Parent));
                });
                collection.OrderBy(x => x.Title);
                collection.Inverse(true);
                collection.Lazy(CollectionLazy.Lazy);
            },
            rel => {
                rel.OneToMany();
            });

            Set(x => x.Questionnaires, collection => {
                collection.Key(c => {
                    c.Column(nameof(QuestionnaireListViewItem.FolderId));
                });
                collection.OrderBy(x => x.Title);
                collection.Inverse(true);
                collection.Lazy(CollectionLazy.Lazy);
            },
            rel => {
                rel.OneToMany();
            });
        }
    }
}
