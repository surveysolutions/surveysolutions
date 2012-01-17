using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Views.Group
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
