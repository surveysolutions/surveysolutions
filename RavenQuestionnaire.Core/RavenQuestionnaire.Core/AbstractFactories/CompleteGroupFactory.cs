using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Views.Group;

namespace RavenQuestionnaire.Core.AbstractFactories
{
    public class CompleteGroupFactory: ICompleteGroupFactory
    {
        public CompleteGroupView CreateGroup(CompleteQuestionnaireDocument doc, CompleteGroup group)
        {
            PropagatableCompleteGroup propagatableGroup = group as PropagatableCompleteGroup;
            if (propagatableGroup != null)
                return new PropagatableCompleteGroupView(doc, propagatableGroup, this);
            return new CompleteGroupView(doc, group, this);
        }
    }
}
