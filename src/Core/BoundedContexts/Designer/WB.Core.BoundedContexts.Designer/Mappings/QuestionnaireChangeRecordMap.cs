using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;

namespace WB.Core.BoundedContexts.Designer.Mappings
{
    public class QuestionnaireChangeRecordMap : ClassMapping<QuestionnaireChangeRecord>
    {
        public QuestionnaireChangeRecordMap()
        {
            Id(x => x.QuestionnaireChangeRecordId, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });
            Property(x => x.QuestionnaireId);
            Property(x => x.ActionType);
            Property(x => x.Timestamp);
            Property(x=>x.Sequence);

            Property(x => x.UserName);
            Property(x => x.UserId);

            Property(x => x.TargetItemId);
            Property(x => x.TargetItemTitle);
            Property(x => x.TargetItemType);

            Set(x => x.References, set =>
            {
                set.Key(key => key.Column("QuestionnaireChangeRecordId"));
                set.Lazy(CollectionLazy.NoLazy);
                set.Cascade(Cascade.All | Cascade.DeleteOrphans);
            },
             relation => relation.OneToMany());
        }

        public class QuestionnaireChangeReferenceMap : ClassMapping<QuestionnaireChangeReference>
        {
            public QuestionnaireChangeReferenceMap()
            {
                Id(x => x.Id, Idmap => Idmap.Generator(Generators.HighLow));
                Property(x => x.ReferenceType);
                Property(x => x.ReferenceId);
                Property(x => x.ReferenceTitle);
                ManyToOne(x => x.QuestionnaireChangeRecord, mto =>
                {
                    mto.Index("QuestionnaireChangeRecords_QuestionnaireChangeReferences");
                    mto.Cascade(Cascade.None);
                });
            }
        }
    }
}
