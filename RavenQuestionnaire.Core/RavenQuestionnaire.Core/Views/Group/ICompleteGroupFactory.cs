using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Views.Group
{
    public interface ICompleteGroupFactory
    {
        CompleteGroupView CreateGroup(CompleteQuestionnaireDocument doc, CompleteGroup group);
    }
}