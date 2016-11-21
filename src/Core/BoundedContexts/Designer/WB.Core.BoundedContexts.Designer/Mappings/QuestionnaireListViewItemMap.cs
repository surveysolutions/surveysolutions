using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Mappings
{
    [PlainStorage]
    public class QuestionnaireListViewItemMap : ClassMapping<QuestionnaireListViewItem>
    {
        public QuestionnaireListViewItemMap()
        {
            Id(x => x.QuestionnaireId, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });

            Property(x => x.PublicId);
            Property(x => x.CreationDate);
            Property(x => x.LastEntryDate);
            Property(x => x.Title);
            Property(x => x.CreatedBy);
            Property(x => x.CreatorName);
            Property(x => x.IsDeleted);
            Property(x => x.IsPublic);
            Property(x => x.Owner);

            Set(x => x.SharedPersons, m =>
            {
                m.Key(keyMap =>
                {
                    keyMap.Column(clm =>
                    {
                        clm.Name("QuestionnaireId");
                    });
                });
                m.Table("SharedPersons");
                m.Lazy(CollectionLazy.NoLazy);
            },
            r => r.Component(e =>
            {
                e.Property(x => x.Id);
                e.Property(x => x.Email);
                e.Property(x => x.IsOwner);
                e.Property(x => x.ShareType);
            }));
        }
    }
}
