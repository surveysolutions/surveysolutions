using System;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Views.Group
{
    public class PropagatableCompleteGroupView : CompleteGroupView
    {
        public PropagatableCompleteGroupView()
        {
        }
        public PropagatableCompleteGroupView(CompleteQuestionnaireStoreDocument doc, ICompleteGroup group, ICompleteGroupFactory groupFactory)
            : base(doc, group, groupFactory)
        {
            this.PropogationPublicKey = group.PropogationPublicKey.Value;
            this.AutoPropagate = group.Propagated== Propagate.AutoPropagated;
        }
        public Guid PropogationPublicKey { get; set; }
        public bool AutoPropagate { get; set; }

        public override string GetClientId(string prefix)
        {
            return string.Format("{0}_{1}_{2}", prefix, this.PublicKey, this.PropogationPublicKey);
        }
    }
}
