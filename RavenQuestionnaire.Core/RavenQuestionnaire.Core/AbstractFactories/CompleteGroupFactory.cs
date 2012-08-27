using System;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Views.Group;

namespace RavenQuestionnaire.Core.AbstractFactories
{
    public class CompleteGroupFactory: ICompleteGroupFactory
    {
        public CompleteGroupView CreateGroup(CompleteQuestionnaireStoreDocument doc, ICompleteGroup group)
        {
       //     PropagatableCompleteGroup propagatableGroup = group as PropagatableCompleteGroup;
            if (group.PropogationPublicKey.HasValue)
                return new PropagatableCompleteGroupView(doc, group, this);
            return new CompleteGroupView(doc, group, this);
        }

        public ICompleteGroup ConvertToCompleteGroup(IGroup group)
        {
            var result = group as ICompleteGroup;
            if (result != null)
                return result;
            var simpleGroup = group as Group;
            if (simpleGroup != null)
                return (CompleteGroup) simpleGroup;
            throw new ArgumentException();
        }
    }
}
