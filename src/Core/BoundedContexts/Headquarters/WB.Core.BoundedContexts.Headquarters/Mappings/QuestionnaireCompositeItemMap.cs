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
            Property(x => x.CascadeFromQuestionId, p => p.Column("cascade_from_question_id"));
            Property(x => x.LinkedToQuestionId, p => p.Column("linked_to_question_id"));
            Property(x => x.LinkedToRosterId, p => p.Column("linked_to_roster_id"));
            Property(x => x.QuestionnaireIdentity);
            Property(x => x.StatExportCaption, p => p.Column("stata_export_caption"));
            Property(x => x.VariableLabel, p => p.Column("variable_label"));
            Property(x => x.QuestionText, p => p.Column("question_text"));

            Bag(x => x.Answers, mapper =>
            {
                mapper.Lazy(CollectionLazy.Extra);
                mapper.Table("questionnaire_entities_answers");
                mapper.Key(k => k.Column("entity_id"));
                mapper.Cascade(Cascade.All);
            }, r => r.Component(c =>
            {
                c.Property(x => x.Text);
                c.Property(x => x.Value);
                c.Property(x => x.Parent);
                c.Property(x => x.AnswerCode, p => p.Column("answer_code"));
                c.Property(x => x.ParentCode, p => p.Column("parent_code"));
            }));
        }
    }

}
