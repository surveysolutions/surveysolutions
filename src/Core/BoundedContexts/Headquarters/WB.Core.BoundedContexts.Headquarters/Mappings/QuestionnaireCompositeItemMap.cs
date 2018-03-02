using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.Infrastructure.PlainStorage;
using WB.Enumerator.Native.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    [PlainStorage]
    public class QuestionnaireCompositeItemMap : ClassMapping<QuestionnaireCompositeItem>
    {
        public QuestionnaireCompositeItemMap()
        {
            Table("questionnaire_entities");
            Schema("readside");
            Id(x => x.Id, map => map.Generator(Generators.Identity));

            Property(x => x.EntityId);
            Property(x => x.Featured);
            Property(x => x.ParentId);
            Property(x => x.QuestionScope, p => p.Column("question_scope"));
            Property(x => x.QuestionType, p => p.Column("question_type"));
            Property(x => x.Type);
            Property(x => x.QuestionnaireIdentity);
        }
    }
}
