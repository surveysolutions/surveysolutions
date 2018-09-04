using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Mappings
{
    [PlainStorage]
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
            Property(x => x.Sequence);

            Property(x => x.UserName, pm => pm.Index("QuestionnaireChangeRecord_UserName"));
            Property(x => x.UserId, pm => pm.Index("QuestionnaireChangeRecord_UserId"));

            Property(x => x.TargetItemId);
            Property(x => x.TargetItemTitle);
            Property(x => x.TargetItemNewTitle);
            Property(x => x.AffectedEntriesCount);
            Property(x => x.TargetItemType);
            Property(x => x.TargetItemDateTime);
            Property(x => x.DiffWithPrevisousVersion);

            Set(x => x.References, set =>
            {
                set.Key(key => key.Column("questionnairechangerecordid"));
                set.Lazy(CollectionLazy.NoLazy);
                set.Cascade(Cascade.All | Cascade.DeleteOrphans);
                set.Inverse(true);
            }, relation => relation.OneToMany());

            Property(x => x.ResultingQuestionnaireDocument, mapping =>
            {
                mapping.Lazy(true);
            });
        }   
    }

    [PlainStorage]
    public class QuestionnaireChangeReferenceMap : ClassMapping<QuestionnaireChangeReference>
    {
        public QuestionnaireChangeReferenceMap()
        {
            Id(x => x.Id, Idmap => Idmap.Generator(Generators.HighLow));
            Property(x => x.ReferenceType);
            Property(x => x.ReferenceId);
            Property(x => x.ReferenceTitle);
            ManyToOne(x => x.QuestionnaireChangeRecord,
                mto =>
                {
                    mto.Column("questionnairechangerecordid");
                    mto.Cascade(Cascade.None);
                    mto.Index("questionnairechangerecords_questionnairechangereferences");
                });
        }
    }
}
