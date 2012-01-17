using Raven.Client;
using RavenQuestionnaire.Core.Documents;

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
            var group = new Entities.Questionnaire(doc).Find<Entities.SubEntities.Group>(input.PublicKey);
            if (group == null)
                return null;
             return new GroupView(doc, group);      
			 }   }
}
