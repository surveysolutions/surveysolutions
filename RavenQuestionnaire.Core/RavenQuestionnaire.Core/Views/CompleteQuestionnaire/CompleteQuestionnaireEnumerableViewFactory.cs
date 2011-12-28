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
            if (!string.IsNullOrEmpty(input.CompleteQuestionnaireId))
            {
                var doc = documentSession.Load<CompleteQuestionnaireDocument>(input.CompleteQuestionnaireId);
                var completeQuestionnaireRoot = new RavenQuestionnaire.Core.Entities.CompleteQuestionnaire(doc);
                RavenQuestionnaire.Core.Entities.SubEntities.Group group = null;

                Iterator<RavenQuestionnaire.Core.Entities.SubEntities.Group, Guid> iterator =
                       new QuestionnaireScreenIterator(completeQuestionnaireRoot);
                if (input.CurrentGroupPublicKey.HasValue)
                {
                    var template = new RavenQuestionnaire.Core.Entities.Questionnaire(doc.Questionnaire);
                    group =
                        template.Find<RavenQuestionnaire.Core.Entities.SubEntities.Group>(
                            input.CurrentGroupPublicKey.Value);
                }
                else if (input.PreviousGroupPublicKey.HasValue)
                {
                   
                    group = input.IsReverse
                                ? iterator.GetPreviousBefoure(input.PreviousGroupPublicKey.Value)
                                : iterator.GetNextAfter(input.PreviousGroupPublicKey.Value);
                }
                else
                {
                    group = input.IsReverse ? iterator.Last : iterator.First;
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
