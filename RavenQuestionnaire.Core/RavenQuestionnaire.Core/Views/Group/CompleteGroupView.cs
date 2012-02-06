using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Answer;
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
            var groupWithQuestions = group as ICompleteGroup<ICompleteGroup, ICompleteQuestion>;
            if (groupWithQuestions != null)
            {

                this.Questions =
                    groupWithQuestions.Questions.Select(
                        q =>
                        new CompleteQuestionView(doc, q)).ToArray();
                this.Groups = groupWithQuestions.Groups.Select(g => groupFactory.CreateGroup(doc, g)).ToArray();
            }
        }
        public virtual string GetClientId(string prefix)
        {
            return string.Format("{0}_{1}", prefix, this.PublicKey);
        }
    }
}
