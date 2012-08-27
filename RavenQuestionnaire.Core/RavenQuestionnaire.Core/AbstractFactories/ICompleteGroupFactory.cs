using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Views.Group;

namespace RavenQuestionnaire.Core.AbstractFactories
{
    public interface ICompleteGroupFactory
    {
        CompleteGroupView CreateGroup(CompleteQuestionnaireStoreDocument doc, ICompleteGroup group);
        ICompleteGroup ConvertToCompleteGroup(IGroup group);
    }
}