using System.Linq;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Core.Views.Group
{
    public class CompleteGroupView : GroupView<CompleteGroupView, CompleteQuestionView, ICompleteGroup, ICompleteQuestion>
    {
        public CompleteGroupView()
        {
        }
        public CompleteGroupView(CompleteQuestionnaireDocument doc, ICompleteGroup group, ICompleteGroupFactory groupFactory)
            : base(doc, group)
        {

                this.ConditionExpression = doc.ConditionExpression;
                this.Questions =
                    group.Children.OfType<ICompleteQuestion>().Select(
                        q => new CompleteQuestionFactory().CreateQuestion(doc, q)).ToArray();
                this.Groups = group.Children.OfType<ICompleteGroup>().Select(g => groupFactory.CreateGroup(doc, g)).ToArray();
            
        }
        public virtual string GetClientId(string prefix)
        {
            return string.Format("{0}_{1}", prefix, this.PublicKey);
        }
    }
}
