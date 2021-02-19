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
            Property(x => x.StataExportCaption, p => p.Column("stata_export_caption"));
            Property(x => x.VariableLabel, p => p.Column("variable_label"));
            Property(x => x.QuestionText, p => p.Column("question_text"));
            Property(x => x.UsedInReporting, p => p.Column("used_in_reporting"));
            Property(x => x.IsFilteredCombobox, p=>p.Column("is_filtered_combobox"));
        }
    }

}
