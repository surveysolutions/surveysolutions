using System;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    public class CompleteQuestionnaireEnumerableViewFactory :
        IViewFactory<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireViewEnumerable>
    {
        private IDocumentSession documentSession;

        public CompleteQuestionnaireEnumerableViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        public CompleteQuestionnaireViewEnumerable Load(CompleteQuestionnaireViewInputModel input)
        {
            var doc = documentSession.Load<CompleteQuestionnaireDocument>(input.CompleteQuestionnaireId);
            var completeQuestionnaireRoot = new RavenQuestionnaire.Core.Entities.CompleteQuestionnaire(doc);

            Iterator<RavenQuestionnaire.Core.Entities.SubEntities.Question, Guid?> iterator =
                new QuestionnaireSimpleIterator(completeQuestionnaireRoot);
            CompleteQuestionView currentViewQuestion = null;
            var question = input.IsReverse
                               ? iterator.GetPreviousBefoure(input.PreviousQuestionPublicKey)
                               : iterator.GetNextAfter(input.PreviousQuestionPublicKey);
            if (question != null)
            {
                currentViewQuestion = new CompleteQuestionView(
                    new QuestionView(question.PublicKey,
                                     question.QuestionText,
                                     question.QuestionType,
                                     question.Answers,
                                     doc.Questionnaire.Id, question.ConditionExpression));
            }
            return new CompleteQuestionnaireViewEnumerable(doc.Id, doc.Questionnaire.Title,
                                                           doc.CompletedAnswers.ToArray(), doc.CreationDate,
                                                           doc.LastEntryDate,
                                                           doc.Status, doc.Creator,
                                                           currentViewQuestion);
        }
    }
}
