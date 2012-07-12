using System;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.ViewSnapshot;

namespace RavenQuestionnaire.Core.Views.Question
{
    public class QuestionViewFactory : IViewFactory<QuestionViewInputModel, QuestionView>
    {
        private IViewSnapshot documentSession;

        public QuestionViewFactory(IViewSnapshot documentSession)
        {
            this.documentSession = documentSession;
        }
         public QuestionView Load(QuestionViewInputModel input)
         {
             var doc = documentSession.ReadByGuid<QuestionnaireDocument>(Guid.Parse(input.QuestionnaireId));

             var question =
                 new RavenQuestionnaire.Core.Entities.Questionnaire(doc).Find
                     <IQuestion>(input.PublicKey);
             if (question == null)
                 return null;


             return new QuestionView(doc, question);

         }
    }
}
