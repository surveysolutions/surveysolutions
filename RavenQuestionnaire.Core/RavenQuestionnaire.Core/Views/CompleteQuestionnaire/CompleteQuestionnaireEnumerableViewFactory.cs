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
                RavenQuestionnaire.Core.Entities.SubEntities.Group group = null;
               
                if (input.CurrentGroupPublicKey.HasValue)
                {
                    var template = new RavenQuestionnaire.Core.Entities.Questionnaire(doc.Questionnaire);
                    group =
                        template.Find<RavenQuestionnaire.Core.Entities.SubEntities.Group>(
                            input.CurrentGroupPublicKey.Value);
                }
                if (group == null)
                {
                    Iterator<RavenQuestionnaire.Core.Entities.SubEntities.Group, Guid?> iterator =
                        new QuestionnaireScreenIterator(completeQuestionnaireRoot);
                    group = input.IsReverse
                                       ? iterator.GetPreviousBefoure(input.PreviousGroupPublicKey)
                                       : iterator.GetNextAfter(input.PreviousGroupPublicKey);
                }
                return new CompleteQuestionnaireViewEnumerable(doc, group);
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
