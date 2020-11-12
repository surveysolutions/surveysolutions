#nullable enable

using System;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public enum EntityType
    {
        Section = 1,
        Question = 2,
        StaticText = 3,
        Variable = 4,
        Roster = 5,
    }

    public static class EntityTypeHelper
    {
        public static EntityType GetEntityType(this IComposite composite)
        {
            if(composite == null) throw new ArgumentException(@"Cannot get entity type from null", nameof(composite));
            var type = composite.GetType();

            if (typeof(IQuestion).IsAssignableFrom(type)) return EntityType.Question;
            if (typeof(IVariable).IsAssignableFrom(type)) return EntityType.Variable;
            if (typeof(IStaticText).IsAssignableFrom(type)) return EntityType.StaticText;
            if (typeof(IGroup).IsAssignableFrom(type)) return EntityType.Section;

            throw new NotSupportedException(composite.GetType().FullName + " is not supported entity type");
        }

        public static EntityType GetEntityType(Guid id, IQuestionnaire questionnaire)
        {
            if (questionnaire.IsQuestion(id)) return EntityType.Question;
            if (questionnaire.IsSubSection(id)) return EntityType.Section;
            if (questionnaire.IsRosterGroup(id)) return EntityType.Roster;
            if (questionnaire.IsStaticText(id)) return EntityType.StaticText;
            if (questionnaire.IsVariable(id)) return EntityType.Variable;

            throw new NotSupportedException($"Entity {id} is not supported entity type");
        }
    }
}
