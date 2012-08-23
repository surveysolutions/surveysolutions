using System;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Views.Question
{
    public class QuestionViewFactory : IViewFactory<QuestionViewInputModel, QuestionView>
    {
        private readonly IDenormalizerStorage<QuestionnaireDocument> _documentSession;

        public QuestionViewFactory(IDenormalizerStorage<QuestionnaireDocument> 
            documentSession)
        {
            this._documentSession = documentSession;
        }
         public QuestionView Load(QuestionViewInputModel input)
         {
             var doc = _documentSession.GetByGuid(Guid.Parse(input.QuestionnaireId));

             var question = new Entities.Questionnaire(doc).Find<IQuestion>(input.PublicKey);
             if (question == null)
                 return null;

             return new QuestionView(doc, question);
         }
    }
}
