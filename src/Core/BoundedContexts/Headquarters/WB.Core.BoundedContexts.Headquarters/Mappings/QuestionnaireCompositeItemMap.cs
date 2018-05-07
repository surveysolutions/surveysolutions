using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    public class QuestionnaireCompositeItemMap : ClassMapping<QuestionnaireCompositeItem>
    {
        public QuestionnaireCompositeItemMap()
        {
            Table("questionnaire_entities");
            Id(x => x.Id, map => map.Generator(Generators.Identity));

            Property(x => x.EntityId);
            Property(x => x.Featured);
            Property(x => x.ParentId);
            Property(x => x.QuestionScope, p => p.Column("question_scope"));
            Property(x => x.QuestionType, p => p.Column("question_type"));
            Property(x => x.EntityType, p => p.Column("entity_type"));
            Property(x => x.QuestionnaireIdentity);
        }
    }
}
