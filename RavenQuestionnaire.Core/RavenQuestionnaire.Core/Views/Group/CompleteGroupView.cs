using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Core.Views.Group
{
    public class CompleteGroupView : GroupView<CompleteGroup, CompleteQuestion, CompleteAnswer>
    {
        public CompleteGroupView()
        {
        }
        public CompleteGroupView(string questionnaireId)
            : base(questionnaireId)
        {
        }
        public CompleteGroupView(CompleteQuestionnaireDocument doc, CompleteGroup group, ICompleteGroupFactory groupFactory)
            : base(doc, group)
        {
            
            this.Questions =
                group.Questions.Select(
                    q =>
                    new CompleteQuestionView(doc, q)).ToArray();
            this.Groups = group.Groups.Select(g => groupFactory.CreateGroup(doc, g)).ToArray();
        }
    }
}
