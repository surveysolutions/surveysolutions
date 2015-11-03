using System;
using Main.Core.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire.Macros
{
    public class MacrosAdded : QuestionnaireEntityEvent
    {
        public MacrosAdded(){ }

        public MacrosAdded(Guid entityId, Guid responsibleId)
        {
            EntityId = entityId;
            ResponsibleId = responsibleId;
        }
    }

    public class MacrosUpdated : QuestionnaireEntityEvent
    {
        public MacrosUpdated() { }

        public MacrosUpdated(Guid entityId, string name, string expression, string description, Guid responsibleId)
        {
            EntityId = entityId;
            ResponsibleId = responsibleId;
            Name = name;
            Expression = expression;
            Description = description;
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Expression { get; set; }
    }

    public class MacrosDeleted : QuestionnaireEntityEvent
    {
        public MacrosDeleted() { }
        public MacrosDeleted(Guid entityId, Guid responsibleId)
        {
            EntityId = entityId;
            ResponsibleId = responsibleId;
        }
    }
}
