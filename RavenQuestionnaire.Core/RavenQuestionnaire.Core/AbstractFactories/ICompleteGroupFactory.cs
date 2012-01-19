using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Views.Group;

namespace RavenQuestionnaire.Core.AbstractFactories
{
    public interface ICompleteGroupFactory
    {
        CompleteGroupView CreateGroup(CompleteQuestionnaireDocument doc, CompleteGroup group);
    }
}