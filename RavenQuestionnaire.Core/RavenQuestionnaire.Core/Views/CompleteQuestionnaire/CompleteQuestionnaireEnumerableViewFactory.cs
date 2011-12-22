using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Views.Questionnaire;

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
            if (!string.IsNullOrEmpty(input.CompleteQuestionnaireId))
            {
                var doc = documentSession.Load<CompleteQuestionnaireDocument>(input.CompleteQuestionnaireId);
                var completeQuestionnaireRoot = new RavenQuestionnaire.Core.Entities.CompleteQuestionnaire(doc);

                Iterator<RavenQuestionnaire.Core.Entities.SubEntities.Question, Guid?> iterator =
                    new QuestionnaireSimpleIterator(completeQuestionnaireRoot);
                var question = input.IsReverse
                                   ? iterator.GetPreviousBefoure(input.PreviousQuestionPublicKey)
                                   : iterator.GetNextAfter(input.PreviousQuestionPublicKey);
                return new CompleteQuestionnaireViewEnumerable(doc, question);
            }
            if (!string.IsNullOrEmpty(input.TemplateQuestionanireId))
            {
                var doc = documentSession.Load<QuestionnaireDocument>(input.TemplateQuestionanireId);
                return new CompleteQuestionnaireViewEnumerable(doc);
            }
            return null;
        }

    }
}
