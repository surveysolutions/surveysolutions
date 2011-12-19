using System.Linq;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Views.Questionnaire;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    public class CompleteQuestionnaireViewFactory: IViewFactory<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireView>
    {
        private IDocumentSession documentSession;

        public CompleteQuestionnaireViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }
        public CompleteQuestionnaireView Load(CompleteQuestionnaireViewInputModel input)
        {
            var doc = documentSession.Load<CompleteQuestionnaireDocument>(input.CompleteQuestionnaireId);
          
            return new CompleteQuestionnaireView(doc.Id,
                                                 new QuestionnaireView(doc.Questionnaire.Id, doc.Questionnaire.Title,
                                                                       doc.Questionnaire.CreationDate,
                                                                       doc.Questionnaire.LastEntryDate,
                                                                       doc.Questionnaire.Questions.Select(
                                                                           q =>
                                                                           new QuestionView(q.PublicKey, q.QuestionText,
                                                                                            q.QuestionType, q.Answers,
                                                                                            doc.Id))
                                                     ), doc.CompletedAnswers.ToArray(), doc.CreationDate, doc.LastEntryDate,
                                                     doc.Status, doc.ResponsibleId);
        }
    
    }
}
