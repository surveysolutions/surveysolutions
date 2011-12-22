using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Core.Views.Group
{
    public class GroupViewFactory: IViewFactory<GroupViewInputModel, GroupView>
    {
         private IDocumentSession documentSession;

         public GroupViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }
         public GroupView Load(GroupViewInputModel input)
        {
            var doc = documentSession.Load<QuestionnaireDocument>(input.QuestionnaireId);
            var group = new RavenQuestionnaire.Core.Entities.Questionnaire(doc).Find<RavenQuestionnaire.Core.Entities.SubEntities.Group>(input.PublickKey);
            if (group == null)
                return null;
             return new GroupView(doc, group);

        }
    }
}
