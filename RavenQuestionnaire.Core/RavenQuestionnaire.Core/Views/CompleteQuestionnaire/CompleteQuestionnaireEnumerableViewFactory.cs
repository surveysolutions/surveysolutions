using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Views.Group;
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
                var completeQuestionnaireRoot = new Entities.CompleteQuestionnaire(doc);
                RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteGroup group = null;

                Iterator<RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteGroup, Guid> iterator =
                    new QuestionnaireScreenIterator(completeQuestionnaireRoot);
                if (input.CurrentGroupPublicKey.HasValue)
                {
                    group =
                        completeQuestionnaireRoot.Find
                            <RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteGroup>(
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
                return new CompleteQuestionnaireViewEnumerable((CompleteQuestionnaireDocument)doc);
            }
            return null;

        }
    }
}
