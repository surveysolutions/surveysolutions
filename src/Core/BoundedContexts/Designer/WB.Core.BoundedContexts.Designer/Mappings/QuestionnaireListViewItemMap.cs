using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;

namespace WB.Core.BoundedContexts.Designer.Mappings
{
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
                        clm.Index("QuestionnaireListViewItem_SharedPersons");
                    });
                });
                m.Table("SharedPersons");
                m.Lazy(CollectionLazy.NoLazy);
            },
            r => r.Element(e =>
            {
                e.Column("SharedPersonId");
            }));
        }
    }
}
